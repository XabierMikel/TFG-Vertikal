﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vertikal.Core.Interfaces;

namespace Vertikal.Services
{
    public class MauiSecureStorageService : ISecureStorageService
    {
        public Task SetAsync(string key, string value) => SecureStorage.SetAsync(key, value);
        public Task<string> GetAsync(string key) => SecureStorage.GetAsync(key);
    }
}
