﻿namespace DotNetRu.Auditor.Storage
{
    public interface IStore
    {
        ISession OpenSession(SessionOptions? sessionOptions = null);
    }
}
