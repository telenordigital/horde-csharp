using System;
using System.Collections.Generic;
using System.Net;
using NBIoT;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void TestClient()
        {
            var _ = new Client();
        }

        [Fact]
        public async void TestTeams()
        {
            var client = new Client();

            var team = await client.CreateTeam(new Team());
            try {
                var teams = await client.GetTeams();
                Assert.Equal(team.ID, Array.Find(teams, t => t.ID == team.ID).ID);

                var tagKey = "test key";
                var tagValue = "test value";
                team.Tags = new Dictionary<string, string>();
                team.Tags[tagKey] = tagValue;
                team = await client.UpdateTeam(team);
                Assert.NotNull(team.Tags);
                Assert.Equal(tagValue, team.Tags[tagKey]);

                var team2 = await client.GetTeam(team.ID);
                Assert.Equal(team.ID, team2.ID);

                ClientException x = await Assert.ThrowsAsync<ClientException>(() => client.UpdateTeamMemberRole(team.ID, team.Members[0].UserID, "member"));
                Assert.Equal(HttpStatusCode.Forbidden, x.Status);

                x = await Assert.ThrowsAsync<ClientException>(() => client.DeleteTeamMember(team.ID, team.Members[0].UserID));
                Assert.Equal(HttpStatusCode.Forbidden, x.Status);

                Invite iv = await client.CreateInvite(team.ID);
                try {
                    Assert.Contains(iv, await client.GetInvites(team.ID));
                    Assert.Equal(iv, await client.GetInvite(team.ID, iv.Code));
                    x = await Assert.ThrowsAsync<ClientException>(() => client.AcceptInvite(iv.Code));
                    Assert.Equal(HttpStatusCode.Conflict, x.Status);
                } finally {
                    await client.DeleteInvite(team.ID, iv.Code);
                }
            } finally {
                await client.DeleteTeam(team.ID);
            }
        }

        [Fact]
        public async void TestCollections()
        {
            var client = new Client();


            var collection = await client.CreateCollection(new Collection());
            try {
                var collections = await client.GetCollections();
                Assert.Equal(collection.ID, Array.Find(collections, t => t.ID == collection.ID).ID);

                var tagKey = "test key";
                var tagValue = "test value";
                collection.Tags = new Dictionary<string, string>();
                collection.Tags[tagKey] = tagValue;
                collection = await client.UpdateCollection(collection);
                Assert.NotNull(collection.Tags);
                Assert.Equal(tagValue, collection.Tags[tagKey]);

                var collection2 = await client.GetCollection(collection.ID);
                Assert.Equal(collection.ID, collection2.ID);
            } finally {
                await client.DeleteCollection(collection.ID);
            }
        }

        [Fact]
        public async void TestDevices()
        {
            var client = new Client();

            var collection = await client.CreateCollection(new Collection());

            try {
                var devices = await client.GetDevices(collection.ID);
                Assert.Empty(devices);

                var device = await client.CreateDevice(collection.ID, new Device { IMEI = "12", IMSI = "34" });
                try {
                    devices = await client.GetDevices(collection.ID);
                    Assert.Single(devices);
                    Assert.Equal(device, devices[0]);

                    var imei = "56";
                    var imsi = "78";
                    var tagKey = "test key";
                    var tagValue = "test value";
                    device.IMEI = imei;
                    device.IMSI = imsi;
                    device.Tags = new Dictionary<string, string>();
                    device.Tags[tagKey] = tagValue;
                    device = await client.UpdateDevice(collection.ID, device);
                    Assert.NotNull(device.Tags);
                    Assert.Equal(imei, device.IMEI);
                    Assert.Equal(imsi, device.IMSI);
                    Assert.Equal(tagValue, device.Tags[tagKey]);

                    var device2 = await client.GetDevice(collection.ID, device.ID);
                    Assert.Equal(device.ID, device2.ID);
                } finally {
                    await client.DeleteDevice(collection.ID, device.ID);
                }

                devices = await client.GetDevices(collection.ID);
                Assert.Empty(devices);
            } finally {
                await client.DeleteCollection(collection.ID);
            }
        }

        [Fact]
        public async void TestOutputs()
        {
            var client = new Client();

            var collection = await client.CreateCollection(new Collection());

            try {
                var outputs = await client.GetOutputs(collection.ID);
                Assert.Empty(outputs);

                var output = (IFTTTOutput)await client.CreateOutput(collection.ID, new IFTTTOutput { Key = "abc", EventName = "def" });
                try {
                    outputs = await client.GetOutputs(collection.ID);
                    Assert.Single(outputs);
                    Assert.Equal(output, outputs[0]);

                    var output2 = (IFTTTOutput)await client.GetOutput(collection.ID, output.ID);
                    Assert.Equal(output.ID, output2.ID);
                } finally {
                    await client.DeleteOutput(collection.ID, output.ID);
                }

                outputs = await client.GetOutputs(collection.ID);
                Assert.Empty(outputs);
            } finally {
                await client.DeleteCollection(collection.ID);
            }
        }
    }
}
