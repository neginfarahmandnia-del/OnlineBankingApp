using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Domain.Models;

namespace OnlineBankingApp.Infrastructure.Services;

public class GreetingService : IGreetingService
{
    public Greeting GetGreeting(string name)
    {
        return new Greeting
        {
            Message = $"Hallo, {name}! Willkommen bei OnlineBankingApp."
        };
    }
}
