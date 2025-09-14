using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Application.Interfaces;

using OnlineBankingApp.Domain.Models;

public interface IGreetingService
{
    Greeting GetGreeting(string name);
}
