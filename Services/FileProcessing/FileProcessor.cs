﻿using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace API_backend.Services.FileProcessing
{
    /// <summary>
    /// Service for aggregating experiment data and parsing the data into a .csv file.
    /// </summary>
    /// <remarks>
    /// Implemented based off of bash scripts provided in the docker-swarm repository.
    /// </remarks>
    /// <seealso href="https://github.com/ThePICARDProject/docker-swarm/"/>
    public class FileProcessor
    {
        private readonly string _databaseFileSystemBasePath;
        private readonly string _dockerPath;

        public FileProcessor(FileProcessorOptions fileProcessorOptions)
        {
            // Check our Docker-Swarm path
            _dockerPath = fileProcessorOptions.DockerSwarmPath;
            if (string.IsNullOrEmpty(_dockerPath))
                throw new ArgumentNullException(nameof(fileProcessorOptions.DockerSwarmPath));
            if (!Directory.Exists(_dockerPath))
                throw new DirectoryNotFoundException($"The directory \"{fileProcessorOptions.DockerSwarmPath}\" could not be found or does not exist.");

            // Initialize the base path for the database filesystem and verify it exists
            _databaseFileSystemBasePath = fileProcessorOptions.DatabaseFileSystemBasePath;
            if (string.IsNullOrEmpty(_databaseFileSystemBasePath))
                throw new ArgumentNullException(nameof(fileProcessorOptions.DatabaseFileSystemBasePath));
            if (!Directory.Exists(_databaseFileSystemBasePath))
                throw new DirectoryNotFoundException($"The output directory \"{_databaseFileSystemBasePath}\" could not be found or does not exist.");
        }

        /// <summary>
        /// Aggregates all data within the Hdfs into a single output text file within the database filesystem.
        /// 
        /// Database filesystem directory structure is as follows: 
        ///     "{DatabaseFileSystemBasePath}/{UserId}/{AlgorithmName}/{Timestamp}_{surveyNumber}.txt"
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="algorithmName"></param>
        /// <param name="experimentNumber"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> AggregateData(string userId, string algorithmName, string surveyNumber, string timestamp)
        {
            // Verify Args
            if(string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if(string.IsNullOrEmpty(algorithmName))
                throw new ArgumentNullException(nameof(algorithmName));
            if(string.IsNullOrEmpty(surveyNumber))
                throw new ArgumentNullException(nameof(surveyNumber));
            if(string.IsNullOrEmpty(timestamp))
                throw new ArgumentNullException(nameof(timestamp));

            // Generate the aggregate file path
            string aggregateFilePath = Path.Combine(new string[] 
            { 
                _databaseFileSystemBasePath, 
                userId, 
                algorithmName,
                $"{timestamp}_{surveyNumber}.txt",
            });
            
            // Currently an idea for using bash based on current implementation. Doesn't seem to be a better option
            using(Process resultsOut = new Process())
            {
                resultsOut.StartInfo.FileName = Path.Combine(_dockerPath, "results-out.sh");

                // Add arguments
                Collection<string> argumentsList = resultsOut.StartInfo.ArgumentList;
                argumentsList.Add(aggregateFilePath);
                resultsOut.StartInfo.CreateNoWindow = false;

                // Start the process and wait to exit
                if (!resultsOut.Start())
                    throw new Exception("Failed to start the new process");
                await resultsOut.WaitForExitAsync();
            }

            // If we exit the process and the file still does not exist, throw an exception
            if (!File.Exists(aggregateFilePath))
                throw new FileNotFoundException($"Failed to aggregate data: output file does not exist at the path \"{aggregateFilePath}\".");
                       
            // Return the path of the saved file
            return aggregateFilePath;   
        }

        /// <summary>
        /// Generates a CSV file containing formatted results.
        /// NOTE: AggregateData must be called before this method.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetCsv()
        {
            throw new NotImplementedException("TODO");
        }
    }
}
