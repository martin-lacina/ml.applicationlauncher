// Copyright © Martin Lacina

using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class RawExecutableProcessStartInfoResolver : StandaloneExecutableProcessStartInfoResolver
{
    public RawExecutableProcessStartInfoResolver()
        : base(ExecutionMode.Raw)
    {
    }

    public override bool CheckExistance => false;
}
