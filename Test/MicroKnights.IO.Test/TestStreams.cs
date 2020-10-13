using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable AccessToDisposedClosure

namespace MicroKnights.IO.Test
{
    public class TestStreams
    {
        private readonly ITestOutputHelper _output;
        private readonly string _workingDirectory = Path.Combine(Path.GetTempPath(), "TestSplitStream");

        public TestStreams(ITestOutputHelper output)
        {
            _output = output;
        }

        private void DeleteTestFiles(string directoryWithFiles)
        {
            var wildcards = "*.*";
            var files = Directory.GetFiles(directoryWithFiles, wildcards);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        private const string Md5 = "9b42ca5c1fb11fe5c9a52c193360de68";
        private const string Sha1 = "1fe0fda07f9a87b6908e69ef586c21b4833acb5";
        private const string Sha256 = "22a84bad77d526b32768ceee50714b954da14d32f0efee5faeb0435f8e1a6f82";
        private const string Sha384 = "6c7da21238be126f16d39ae129619eb3a18a4d69c2d835eaf99c69a2bdc3b2e7a1bd41dd310da92e97bf795f07177a7";
        private const string Sha512 = "3f15a970d9115ef238cc2b6ecc2ae2854b8914016f71d8465d59d77d7f763c6d7316a7b799035bc12d2bbd5186ae67de473cfb7f53ddc5c672b848943bbccfb";

        [Fact]
        public async Task TestParallelStreams()
        {
            Directory.CreateDirectory(_workingDirectory);
            DeleteTestFiles(_workingDirectory);

            string checksumMd5 = "";
            string checksumSha1 = "";
            string checksumSha256 = "";
            string checksumSha384 = "";
            string checksumSha512 = "";

            byte[] fileBytes;
            var assembly = GetType().GetTypeInfo().Assembly;
            using (var inputSourceStream = assembly.GetManifestResourceStream(string.Join(".", GetType().Namespace, "Files", "ST_5419_2016_INIT_EN.pdf")))
            using (var inputMemoryStream = new MemoryStream(ushort.MaxValue))
            {
                // ReSharper disable once PossibleNullReferenceException
                await inputSourceStream.CopyToAsync(inputMemoryStream);
                fileBytes = inputMemoryStream.ToArray();
            }


            using (var inputSourceStream = new MemoryStream(fileBytes))
            // Source.Reader faster then Split.Readers
            using (var inputSplitStream = new DebuggableReadableSplitStream(0, 2345, inputSourceStream))
            // Source.Reader slower then Split.Readers
            //            using (var splitStream = new DebuggableReadableSplitStream(2345,234,inputStream))

            using (var inputFileStream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputFileStream = File.OpenWrite(Path.Combine(_workingDirectory, "outputTestFile.pdf")))

            using (var inputStress1Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var inputStress2Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var inputStress3Stream = inputSplitStream.GetForwardReadOnlyStream())

            using (var inputMd5Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMd5Stream = MD5.Create())

            using (var inputSha1Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha1Stream = SHA1.Create())

            using (var inputSha256Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha256Stream = SHA256.Create())

            using (var inputSha384Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha384Stream = SHA384.Create())

            using (var inputSha512Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha512Stream = SHA512.Create())

            using (var inputMemoryStream1 = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMemoryStream1 = new MemoryStream())

            using (var inputMemoryStream2 = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMemoryStream2 = new MemoryStream())

            using (var inputMemoryStream3 = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMemoryStream3 = new MemoryStream())
            {
                var readAheadTask = inputSplitStream.StartReadAhead();

                Parallel.Invoke(() =>
                {
                    var bytes = outputSha1Stream.ComputeHash(inputSha1Stream);
                    checksumSha1 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA1_{checksumSha1}.bin"); // 1fe0fda07f9a87b6908e69ef586c21b4833acb5
                    File.WriteAllBytes(filename, bytes);
                },
                () =>
                {
                    inputStress1Stream.CopyTo(Stream.Null);
                },
                () =>
                {
                    var bytes = outputSha512Stream.ComputeHash(inputSha512Stream);
                    checksumSha512 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA512_{checksumSha512}.bin"); // 3f15a970d9115ef238cc2b6ecc2ae2854b8914016f71d8465d59d77d7f763c6d7316a7b799035bc12d2bbd5186ae67de473cfb7f53ddc5c672b848943bbccfb
                    File.WriteAllBytes(filename, bytes);
                },
                () =>
                {
                    var buffer = new byte[1234];
                    var length = -1;
                    while (length != 0)
                    {
                        length = inputMemoryStream1.Read(buffer, 0, buffer.Length);
                        if (length > 0)
                        {
                            Thread.Sleep(42);
                            outputMemoryStream1.Write(buffer, 0, length);
                        }
                    }
                },
                () =>
                {
                    inputStress2Stream.CopyTo(Stream.Null);
                },
                () =>
                {
                    var buffer = new byte[42];
                    var length = -1;
                    while (length != 0)
                    {
                        length = inputMemoryStream2.Read(buffer, 0, buffer.Length);
                        if (length > 0)
                        {
                            outputMemoryStream2.Write(buffer, 0, length);
                        }
                    }
                },
                () =>
                {
                    var bytes = outputSha256Stream.ComputeHash(inputSha256Stream);
                    checksumSha256 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA256_{checksumSha256}.bin"); // 22a84bad77d526b32768ceee50714b954da14d32f0efee5faeb0435f8e1a6f82 
                    File.WriteAllBytes(filename, bytes);
                },
                () =>
                {
                    var bytes = outputMd5Stream.ComputeHash(inputMd5Stream);
                    checksumMd5 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"MD5_{checksumMd5}.bin"); // 9b42ca5c1fb11fe5c9a52c193360de68
                    File.WriteAllBytes(filename, bytes);
                },
                () =>
                {
                    inputStress3Stream.CopyTo(Stream.Null);
                },
                () =>
                {
                    var bytes = outputSha384Stream.ComputeHash(inputSha384Stream);
                    checksumSha384 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA384_{checksumSha384}.bin"); // 6c7da21238be126f16d39ae129619eb3a18a4d69c2d835eaf99c69a2bdc3b2e7a1bd41dd310da92e97bf795f07177a7
                    File.WriteAllBytes(filename, bytes);
                },
                () =>
                {
                    var buffer = new byte[7];
                    var length = -1;
                    while (length != 0)
                    {
                        length = inputMemoryStream3.Read(buffer, 0, buffer.Length);
                        if (length > 0)
                        {
                            outputMemoryStream3.Write(buffer, 0, length);
                        }
                    }
                },
                () =>
                {
                    inputFileStream.CopyTo(outputFileStream);
                });

                await readAheadTask; // we only wait, if exceptions is thrown.
                Assert.True(readAheadTask.IsCompletedSuccessfully, "IsCompletedSuccessfully");

                Assert.True(checksumMd5 == Md5, "MD5 Error");
                Assert.True(checksumSha1 == Sha1, "Sha1 Error");
                Assert.True(checksumSha256 == Sha256, "Sha256 Error");
                Assert.True(checksumSha512 == Sha512, "Sha512 Error");
                Assert.True(checksumSha384 == Sha384, "Sha384 Error");

                Assert.Equal(fileBytes, outputMemoryStream1.ToArray());
                Assert.Equal(fileBytes, outputMemoryStream2.ToArray());
                Assert.Equal(fileBytes, outputMemoryStream3.ToArray());

                _output.WriteLine($"WorkingDirectory: {_workingDirectory}");
            }
        }

        [Fact]
        public async Task TestTaskStreams()
        {
            Directory.CreateDirectory(_workingDirectory);
            DeleteTestFiles(_workingDirectory);

            string checksumMd5 = "";
            string checksumSha1 = "";
            string checksumSha256 = "";
            string checksumSha384 = "";
            string checksumSha512 = "";

            byte[] fileBytes;
            var assembly = GetType().GetTypeInfo().Assembly;
            using (var inputSourceStream = assembly.GetManifestResourceStream(string.Join(".", GetType().Namespace, "Files", "ST_5419_2016_INIT_EN.pdf")))
            using (var inputMemoryStream = new MemoryStream(ushort.MaxValue))
            {
                // ReSharper disable once PossibleNullReferenceException
                await inputSourceStream.CopyToAsync(inputMemoryStream);
                fileBytes = inputMemoryStream.ToArray();
            }

            var tasks = new List<Task>();
            using (var inputSourceStream = new MemoryStream(fileBytes))
            // Source.Reader faster then Split.Readers
            using (var inputSplitStream = new DebuggableReadableSplitStream(0, 2345, inputSourceStream))
            // Source.Reader slower then Split.Readers
            //            using (var splitStream = new DebuggableReadableSplitStream(2345,234,inputStream))

            using (var inputFileStream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputFileStream = File.OpenWrite(Path.Combine(_workingDirectory, "outputTestFile.pdf")))

            using (var inputStress1Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var inputStress2Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var inputStress3Stream = inputSplitStream.GetForwardReadOnlyStream())

            using (var inputMd5Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMd5Stream = MD5.Create())

            using (var inputSha1Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha1Stream = SHA1.Create())

            using (var inputSha256Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha256Stream = SHA256.Create())

            using (var inputSha384Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha384Stream = SHA384.Create())

            using (var inputSha512Stream = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputSha512Stream = SHA512.Create())

            using (var inputMemoryStream1 = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMemoryStream1 = new MemoryStream())

            using (var inputMemoryStream2 = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMemoryStream2 = new MemoryStream())

            using (var inputMemoryStream3 = inputSplitStream.GetForwardReadOnlyStream())
            using (var outputMemoryStream3 = new MemoryStream())
            {
                var readAheadTask = inputSplitStream.StartReadAhead();

                tasks.Add(Task.Run(() =>
                {
                    var bytes = outputSha1Stream.ComputeHash(inputSha1Stream);
                    checksumSha1 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA1_{checksumSha1}.bin"); // 1fe0fda07f9a87b6908e69ef586c21b4833acb5
                    File.WriteAllBytes(filename, bytes);
                }));
                tasks.Add(Task.Run(() =>
                {
                    inputStress1Stream.CopyTo(Stream.Null);
                }));
                tasks.Add(Task.Run(() =>
                {
                    var bytes = outputSha512Stream.ComputeHash(inputSha512Stream);
                    checksumSha512 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA512_{checksumSha512}.bin"); // 3f15a970d9115ef238cc2b6ecc2ae2854b8914016f71d8465d59d77d7f763c6d7316a7b799035bc12d2bbd5186ae67de473cfb7f53ddc5c672b848943bbccfb
                    File.WriteAllBytes(filename, bytes);
                }));
                tasks.Add(Task.Run(() =>
                {
                    var buffer = new byte[1234];
                    var length = -1;
                    while (length != 0)
                    {
                        length = inputMemoryStream1.Read(buffer, 0, buffer.Length);
                        if (length > 0)
                        {
                            Thread.Sleep(42);
                            outputMemoryStream1.Write(buffer, 0, length);
                        }
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    inputStress2Stream.CopyTo(Stream.Null);
                }));
                tasks.Add(Task.Run(() =>
                {
                    var buffer = new byte[42];
                    var length = -1;
                    while (length != 0)
                    {
                        length = inputMemoryStream2.Read(buffer, 0, buffer.Length);
                        if (length > 0)
                        {
                            outputMemoryStream2.Write(buffer, 0, length);
                        }
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    var bytes = outputSha256Stream.ComputeHash(inputSha256Stream);
                    checksumSha256 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA256_{checksumSha256}.bin"); // 22a84bad77d526b32768ceee50714b954da14d32f0efee5faeb0435f8e1a6f82 
                    File.WriteAllBytes(filename, bytes);
                }));
                tasks.Add(Task.Run(() =>
                {
                    var bytes = outputMd5Stream.ComputeHash(inputMd5Stream);
                    checksumMd5 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"MD5_{checksumMd5}.bin"); // 9b42ca5c1fb11fe5c9a52c193360de68
                    File.WriteAllBytes(filename, bytes);
                }));
                tasks.Add(Task.Run(() =>
                {
                    inputStress3Stream.CopyTo(Stream.Null);
                }));
                tasks.Add(Task.Run(() =>
                {
                    var bytes = outputSha384Stream.ComputeHash(inputSha384Stream);
                    checksumSha384 = string.Join("", bytes.Select(x => x.ToString("x")));
                    var filename = Path.Combine(_workingDirectory, $"SHA384_{checksumSha384}.bin"); // 6c7da21238be126f16d39ae129619eb3a18a4d69c2d835eaf99c69a2bdc3b2e7a1bd41dd310da92e97bf795f07177a7
                    File.WriteAllBytes(filename, bytes);
                }));
                tasks.Add(Task.Run(() =>
                {
                    var buffer = new byte[7];
                    var length = -1;
                    while (length != 0)
                    {
                        length = inputMemoryStream3.Read(buffer, 0, buffer.Length);
                        if (length > 0)
                        {
                            outputMemoryStream3.Write(buffer, 0, length);
                        }
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    inputFileStream.CopyTo(outputFileStream);
                }));

                await readAheadTask; // we only wait, if exceptions is thrown.
                Task.WaitAll(tasks.ToArray());

                Assert.True(readAheadTask.IsCompletedSuccessfully, "IsCompletedSuccessfully");

                Assert.True(checksumMd5 == Md5, "MD5 Error");
                Assert.True(checksumSha1 == Sha1, "Sha1 Error");
                Assert.True(checksumSha256 == Sha256, "Sha256 Error");
                Assert.True(checksumSha512 == Sha512, "Sha512 Error");
                Assert.True(checksumSha384 == Sha384, "Sha384 Error");

                Assert.Equal(fileBytes, outputMemoryStream1.ToArray());
                Assert.Equal(fileBytes, outputMemoryStream2.ToArray());
                Assert.Equal(fileBytes, outputMemoryStream3.ToArray());

                _output.WriteLine($"WorkingDirectory: {_workingDirectory}");
            }
        }


    }
}
