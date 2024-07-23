using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder.Tests {
    [TestClass]
    public class UnitTest1 {
        private static async Task<bool> Exists(string url) {
            try {
                var req = WebRequest.Create(url);
                req.Method = "HEAD";
                req.Timeout = 3000;
                using var resp = await req.GetResponseAsync();
                if ((resp as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound) {
                    return true;
                }
            } catch (Exception) { }
            return false;
        }

        private static async Task TestUrl(double? expected, string url) {
            DateTime start = DateTime.Now;
            TimeSpan? duration = await Providers.All.GetDurationAsync(new Uri(url));
            if (expected is double d) {
                if (duration is TimeSpan ts) {
                    Assert.AreEqual(d, ts.TotalSeconds, 0.1);
                } else if (await Exists(url)) {
                    Assert.Inconclusive();
                } else {
                    Assert.IsNotNull(duration);
                }
            } else {
                Assert.IsNull(duration);
            }

            TimeSpan maxts = TimeSpan.FromSeconds(15);
            if (DateTime.Now - start > maxts)
                Assert.Inconclusive($"Took more than {maxts}");
        }

        [TestMethod]
        public async Task TestYouTube_1() {
            await TestUrl(424, "https://youtu.be/bMZIG-iMS9k");
            await TestUrl(424, "https://www.youtube.com/watch?v=bMZIG-iMS9k");
        }

        [TestMethod]
        public async Task TestYouTube_2() {
            await TestUrl(424, "https://www.youtube.com/embed/bMZIG-iMS9k");
        }

        [TestMethod]
        public async Task TestYouTube_3() {
            await TestUrl(null, "https://www.example.com/?v=bMZIG-iMS9k");
        }

        [TestMethod]
        public async Task TestYouTube_4() {
            await TestUrl(null, "https://www.youtube.com/watch?v=rrjwqF4yJ9w");
        }

        [TestMethod]
        public async Task TestRedirect_1() {
            await TestUrl(424, "https://tinyurl.com/mshmkvyf"); 
        }

        [TestMethod]
        public async Task TestRedirect_2() {
            await TestUrl(210, "https://tinyurl.com/3e7wahxj");
        }

        [TestMethod]
        public async Task TestMP4_1() {
            await TestUrl(596.474195, "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4");
        }

        [TestMethod]
        public async Task TestMP4_2() {
            await TestUrl(653.804263, "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4");
        }

        [TestMethod]
        public async Task TestMP4_3() {
            await TestUrl(29.568, "https://archive.org/download/SampleVideo1280x7205mb/SampleVideo_1280x720_5mb.mp4");
        }

        [TestMethod]
        public async Task TestVorbis_1() {
            await TestUrl(2237440.0 / 48000.0, "https://www.lakora.us/durationfinder/sample_960x400_ocean_with_audio.ogv");
        }

        [TestMethod]
        public async Task TestVorbis_2() {
            await TestUrl(10780810.0 / 44100.0, "https://www.lakora.us/durationfinder/sample4.ogg");
        }

        [TestMethod]
        public async Task TestHLS_1() {
            await TestUrl(210, "https://bitdash-a.akamaihd.net/content/MI201109210084_1/m3u8s/f08e80da-bf1d-4e3d-8899-f0f6155f6efa.m3u8");
        }

        [TestMethod]
        public async Task TestHLS_2() {
            await TestUrl(600, "https://devstreaming-cdn.apple.com/videos/streaming/examples/img_bipbop_adv_example_ts/master.m3u8");
        }

        [TestMethod]
        public async Task TestHLS_3() {
            await TestUrl(null, "https://cph-p2p-msl.akamaized.net/hls/live/2000341/test/master.m3u8");
        }

        [TestMethod]
        public async Task TestVimeo_1() {
            await TestUrl(57, "https://vimeo.com/241309009");
        }

        [TestMethod]
        public async Task TestVimeo_2() {
            await TestUrl(536, "https://vimeo.com/181964440");
        }

        [TestMethod]
        public async Task TestTwitch() {
            await TestUrl(60 * 19 + 24, "https://www.twitch.tv/videos/788022513");
        }

        [TestMethod]
        public async Task TestSoundCloud_1() {
            await TestUrl(60 * 39 + 1, "https://soundcloud.com/lingthusiasm/94-the-perfectly-imperfect-aspect-episode");
        }

        [TestMethod]
        public async Task TestCircular_1() {
            await TestUrl(715, "https://www.lakora.us/durationfinder/circular1.html");
        }

        [TestMethod]
        public async Task TestCircular_2() {
            await TestUrl(715, "https://www.lakora.us/durationfinder/circular2.html");
        }

        [TestMethod]
        public async Task TestCircular_3() {
            await TestUrl(715, "https://www.lakora.us/durationfinder/circular3.html");
        }

        [TestMethod]
        public async Task TestCircular_A() {
            await TestUrl(null, "https://www.lakora.us/durationfinder/circularA.html");
        }

        [TestMethod]
        public async Task TestCircular_B() {
            await TestUrl(null, "https://www.lakora.us/durationfinder/circularB.html");
        }

        private static async Task TestFile(double? expected, string path) {
            path = Path.Combine("..", "..", "..", path);
            DateTime start = DateTime.Now;
            TimeSpan? duration = await Providers.All.GetDurationAsync(new StreamDataSource(new FileStream(path, FileMode.Open, FileAccess.Read)));
            if (expected is double d) {
                if (duration is TimeSpan ts) {
                    Assert.AreEqual(d, ts.TotalSeconds, 0.1);
                } else if (File.Exists(path)) {
                    Assert.Inconclusive();
                } else {
                    Assert.IsNotNull(duration);
                }
            } else {
                Assert.IsNull(duration);
            }

            TimeSpan maxts = TimeSpan.FromSeconds(15);
            if (DateTime.Now - start > maxts)
                Assert.Inconclusive($"Took more than {maxts}");
        }

        [TestMethod]
        public async Task TestMP4_File() {
            await TestFile(29.568, "SampleVideo_1280x720_5mb.mp4");
        }

        [TestMethod]
        public async Task TestVorbis_File() {
            await TestFile(2237440.0 / 48000.0, "sample_960x400_ocean_with_audio.ogv");
        }

        [TestMethod]
        public async Task TestHLS_File() {
            await TestFile(600, "remote_chunklist.m3u8");
        }

        [TestMethod]
        public async Task TestSchemaOrg_File() {
            await TestFile(3 * 60 + 56, "schemaorg.html");
        }

        [TestMethod]
        public async Task TestOEmbed_File() {
            await TestFile(57, "oembed.html");
        }
    }
}