using ConsoleApp1;
using EventChannelBuilder.Abstract;
using EventLogger;
using Events;
using Handlers;

using Microsoft.Extensions.DependencyInjection;

const string endpoint = "opc.tcp://TermoV6.tech.ptk.tomsk.ru:49320";
const string tagId = "ns=2;s=MODBUSTCP.VSN3.SettingBunker1";
const string appId = "testClient";

var provider = new ServiceCollection()
    .AddSingleton<IGenericEventDispatcherLogger, MyLogger>()
    .AddOpcBackend()
    .BuildServiceProvider();

var channelBuilder = provider.GetRequiredService<IChannelBuilder<OpcCommandEvent>>();

var opcChannel = channelBuilder
    .Unbounded()
    .WithMultipleWriters()
    .WithReadersCount(1)
    .Build();

opcChannel.Start();

await opcChannel.Enqueue(new RegisterSession(appId, endpoint));
await opcChannel.Enqueue(new AddItemToSubscription(appId, tagId, "1", (item, e) =>
{
    foreach (var value in item.DequeueValues())
        Console.WriteLine(
            $"Event 1 " +
            $"{DateTime.Now:HH:mm:ss} " +
            $"{item.DisplayName} = {value.Value}"
        );
}));


Console.ReadLine();



