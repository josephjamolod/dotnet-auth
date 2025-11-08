using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Helpers.HelperObjects
{
    public class OperationResult<T, E>
    {
        public T? Value { get; }
        public E? Error { get; }
        public bool IsSuccess { get; }

        private OperationResult(T value)
        {
            Value = value;
            IsSuccess = true;
        }

        private OperationResult(E error)
        {
            Error = error;
            IsSuccess = false;
        }

        public static OperationResult<T, E> Success(T value) => new(value);
        public static OperationResult<T, E> Failure(E error) => new(error);
    }
}