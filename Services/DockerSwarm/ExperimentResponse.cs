﻿namespace API_backend.Services.Docker_Swarm
{
    public class ExperimentResponse
    {
        public int? ErrorCode {  get; set; }
        public string? ErrorMessage { get; set; }
        public string? OutputPath { get; set; }
    }
}