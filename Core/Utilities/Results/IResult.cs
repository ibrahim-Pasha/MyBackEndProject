﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Utilities.Results
{
    //Temel voidler için Başlangıç
    public interface IResult
    {
        bool Success { get; }
        string Massage { get; }
    }
}