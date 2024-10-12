#!/bin/bash

# Executes a single experiment and exports data in the experiment path
# Based on instructions from hadoop-startup.md, autotest.sh, ...

# Get command line arg:
dataset_path=$1
dataset_name=$2
node_count=$3
driver_memory=$4
driver_cores=$5
executer_number=$6
executer_cores=$7
executer_memory=$8
memory_overhead=$9

class_name=${10}
jar_path=${11}

hdfs_output_directory=${12}
results_output_directory=${13}
output_name=${14}

echo "-----Attempting to add nodes-----"
docker stack deploy -c docker-compose.yml "$(basename $(pwd) | sed 's/\./_/g')"
docker service scale "$(basename $(pwd) | sed 's/\./_/g')_worker"=0
docker service update --mount-add 'type=volume,source=datanode-vol-SERV{{.Service.Name}}-NODE{{.Node.ID}}-TASK{{.Task.Slot}},target=/opt/hadoop/data' "$(basename $(pwd) | sed 's/\./_/g')"


echo "-----Attempting to setup hadoop-----"
docker exec "$(docker inspect --format '{{.Status.ContainerStatus.ContainerID}}' "$(docker service ps -q "$(basename $(pwd) | sed 's/\./_/g')_master" --filter desired-state=running)")" \
       	sh -c 'hdfs dfs -mkdir -p /user/hadoop && hdfs dfs -chown hadoop:hadoop /user/hadoop'


echo "-----Attempting to scale node count to ${node_count}-----"
docker service scale "$(basename $(pwd) | sed 's/\./_/g')_worker"="${node_count}"


echo "-----Giving time for Hadoop to come online-----"
sleep 15
# Force hadoop out of safemode REMOVE LATER
docker run --rm --name delete_dataset --network "$(basename $(pwd) | sed 's/\./_/g')_cluster-network" \
	spark-hadoop:latest hdfs dfsadmin -safemode leave

# Add data set
echo "-----Attempting to add data set-----"
docker run --rm --name dataset-injector --network "$(basename $(pwd) | sed 's/\./_/g')_cluster-network" -v "${dataset_path}:/mnt/data" spark-hadoop:latest hdfs dfs -put "/mnt/data/${dataset_name}" /user/hadoop

echo "-----Beginning experiment-----"
docker exec "$(docker inspect --format '{{.Status.ContainerStatus.ContainerID}}' "$(docker service ps -q "$(basename $(pwd) | sed 's/\./_/g')_master" --filter desired-state=running)")" /opt/spark/bin/spark-submit \
    --master yarn \
    --driver-memory "$driver_memory" \
    --driver-cores $driver_cores \
    --num-executors $executer_number \
    --executor-cores $executer_cores \
    --executor-memory "$executer_memory" \
    --conf spark.executor.memoryOverhead=$memory_overhead \
    --class "$class_name" "/opt/jars/$jar_path" $dataset_name $hdfs_output_directory $output_name ${@:15}


echo "-----Attempting to output results for experiment-----"
if [ ! -d $results_output_directory ] ; then
	mkdir $results_output_directory
fi

fileowner="$(stat -c '%U' "${results_output_directory}")"
if [ "${fileowner}" != "justinh225" ] ; then 
	sudo chown -R justinh225 "${results_output_directory}"
fi
docker run --rm --name results-extractor --network "$(basename $(pwd) | sed 's/\./_/g')_cluster-network" -v "${results_output_directory}:/mnt/results" \
    spark-hadoop:latest hdfs dfs -getmerge ${hdfs_output_directory}/${output_name} /mnt/results/${output_name}


echo "-----Cleaning up experiment files-----"
echo "-----Attempting to delete the dataset-----"
docker run --rm --name delete_dataset --network "$(basename $(pwd) | sed 's/\./_/g')_cluster-network" \
	    spark-hadoop:latest hdfs dfs -rm $dataset_name


echo "-----Attempting to shutdown containers-----"
docker stack rm "$(basename $(pwd) | sed 's/\./_/g')"