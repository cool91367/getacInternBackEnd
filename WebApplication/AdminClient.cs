using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace WebApplication.Kafka
{
    /*
        Use admin client to fetch server related data.
        
     */
    public class AdminClient
    {
        static string ToString(int[] array) => $"[{string.Join(", ", array)}]";

        public static void ListGroups(string bootstrapServers)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build())
            {
                // Warning: The API for this functionality is subject to change.
                var groups = adminClient.ListGroups(TimeSpan.FromSeconds(10));
                Console.WriteLine($"Consumer Groups:");
                foreach (var g in groups)
                {
                    Console.WriteLine($"  Group: {g.Group} {g.Error} {g.State}");
                    Console.WriteLine($"  Broker: {g.Broker.BrokerId} {g.Broker.Host}:{g.Broker.Port}");
                    Console.WriteLine($"  Protocol: {g.ProtocolType} {g.Protocol}");
                    Console.WriteLine($"  Members:");
                    foreach (var m in g.Members)
                    {
                        Console.WriteLine($" {m.MemberId} {m.ClientId} {m.ClientHost}");
                        Console.WriteLine($" Metadata: {m.MemberMetadata.Length} bytes");
                        Console.WriteLine($" Assignment: {m.MemberAssignment.Length} bytes");
                    }
                }
            }
        }
        public static List<string> getMetadata(string bootstrapServers)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build())
            {
                List<string> TopicList = new List<string>();
                // Warning: The API for this functionality is subject to change.
                var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(20));
                Console.WriteLine($"{meta.OriginatingBrokerId} {meta.OriginatingBrokerName}: data fetched");

                meta.Topics.ForEach(topic =>
                {
                    TopicList.Add(topic.Topic);

                    //Note: partition & Replicas information can be found here.
                    /* 
                    Console.WriteLine ($"Topic: {topic.Topic} {topic.Error}");
                    topic.Partitions.ForEach (partition => {
                        Console.WriteLine ($"  Partition: {partition.PartitionId}");
                        Console.WriteLine ($"  Replicas: {ToString(partition.Replicas)}");
                        Console.WriteLine ($"  InSyncReplicas: {ToString(partition.InSyncReplicas)}");
                    });*/
                });

                //Note: Broker information can be found here
                /* 
                meta.Brokers.ForEach (broker =>
                    Console.WriteLine ($"Broker: {broker.BrokerId} {broker.Host}:{broker.Port}"));
                */
                return TopicList;

            }
        }
        public static List<Confluent.Kafka.GroupInfo> getGroups(string bootstrapServers)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build())
            {
                // Warning: The API for this functionality is subject to change.
                var groups = adminClient.ListGroups(TimeSpan.FromSeconds(10));
                List<Confluent.Kafka.GroupInfo> GroupList = new List<Confluent.Kafka.GroupInfo>();
                Console.WriteLine($"Consumer Groups:");

                foreach (var g in groups)
                {
                    Console.WriteLine($"  Group: {g.Group} {g.Error} {g.State}");
                    Console.WriteLine($"  Broker: {g.Broker.BrokerId} {g.Broker.Host}:{g.Broker.Port}");
                    Console.WriteLine($"  Protocol: {g.ProtocolType} {g.Protocol}");
                    GroupList.Add(g);

                    //Collect Member-info here if needed.
                    /*
                    Console.WriteLine ($"  Members:");
                    foreach (var m in g.Members) {
                        Console.WriteLine ($"    {m.MemberId} {m.ClientId} {m.ClientHost}");
                        Console.WriteLine ($"    Metadata: {m.MemberMetadata.Length} bytes");
                        Console.WriteLine ($"    Assignment: {m.MemberAssignment.Length} bytes");
                    } */
                }
                return GroupList;
            }

        }

        static async Task CreateTopicAsync(string bootstrapServers, string topicName)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build())
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new TopicSpecification[] {
                        new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 }
                    });
                }
                catch (CreateTopicsException e)
                {
                    Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                }
            }
        }
        public static List<string> RemoveAdminTopics(List<string> TopicList)
        {
            string[] Removee = { "__consumer_offsets" };
            foreach (string RemoveeTopic in Removee)
            {
                if (TopicList.Contains(RemoveeTopic))
                    TopicList.Remove(RemoveeTopic);
            }
            return TopicList;
        }
    }
}
