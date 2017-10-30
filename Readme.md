# SplitStream

Split up the inbounding stream into multiple streams that can be read concurrently.

```
    using (var inputSplitStream = new ReadableSplitStream(inputSourceStream))

    using (var inputFileStream = inputSplitStream.GetForwardReadOnlyStream())
    using (var outputFileStream = File.OpenWrite("MyFileOnAnyFilestore.bin"))

    using (var inputSha1Stream = inputSplitStream.GetForwardReadOnlyStream())
    using (var outputSha1Stream = SHA1.Create())
    {
        inputSplitStream.StartReadAhead();

        Parallel.Invoke(
            () => {
                var bytes = outputSha1Stream.ComputeHash(inputSha1Stream);
                var checksumSha1 = string.Join("", bytes.Select(x => x.ToString("x")));
            },
            () => {
                inputFileStream.CopyTo(outputFileStream);
            },
        );
    }
```

This example shows how 1 stream is read by 2 in parallel, and you can of cause have many more in parallel if needed.

So whenever a new stream is needed simply call `GetForwardReadOnlyStream()` on the `ReadableSplitStream`.

Please notice that the party starts when `StartReadAhead()` is called, else....

## Nuget package
```
PM> Install-Package MicroKnights.IO.SplitStream -Version 1.0.0
```
