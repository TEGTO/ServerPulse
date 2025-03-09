using Amazon.CDK.AWS.MSK;
using Amazon.CDK.CustomResources;
using Constructs;
using System.Collections.Generic;

namespace Cdk
{
    public class KafkaBootstrapResourceProps
    {
        public CfnCluster KafkaCluster { get; set; }
    }

    public class KafkaBootstrapResource : Construct
    {
        public string BootstrapServers { get; private set; }

        public KafkaBootstrapResource(Construct scope, string id, KafkaBootstrapResourceProps props)
            : base(scope, id)
        {
            var getBootstrapBrokers = new AwsSdkCall
            {
                Service = "Kafka",
                Action = "getBootstrapBrokers",
                Parameters = new Dictionary<string, object>
                {
                    { "ClusterArn", props.KafkaCluster.AttrArn }
                },
                PhysicalResourceId = PhysicalResourceId.Of($"getMSKBootstrapBrokers")
            };

            var getBootstrapBrokersResource = new AwsCustomResource(this, "GetKafkaBootstrapBrokers", new AwsCustomResourceProps
            {
                OnUpdate = getBootstrapBrokers,
                Policy = AwsCustomResourcePolicy.FromSdkCalls(new SdkCallsPolicyOptions
                {
                    Resources = ["*"]
                })
            });

            getBootstrapBrokersResource.Node.AddDependency(props.KafkaCluster);

            BootstrapServers = getBootstrapBrokersResource.GetResponseField("BootstrapBrokerString");
        }
    }
}
