using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace payroll.model;

public class Employee
{
    [Key]
    public  string EmployeeNumber { get; set; } = string.Empty;
    
    public required string EmployeeName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public required DateOnly BirthDate { get; set; }


    public required decimal DailyRate { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required WorkSchedule WorkSchedule { get; set; }
}

public enum WorkSchedule
{
    MWF=1, 
    TTHS=2
}