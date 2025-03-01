using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.IAM;
using Constructs;
using System.Collections.Generic;
using Cluster = Amazon.CDK.AWS.ECS.Cluster;
using ClusterProps = Amazon.CDK.AWS.ECS.ClusterProps;

namespace Cdk
{
    public class ServerPulseCdkStack : Stack
    {
        internal ServerPulseCdkStack(Construct scope, string id, ServerPulseStackProps props, DockerImages images) : base(scope, id, props)
        {
            #region [Infrastructure]

            AddKafka();
            AddDb();
            AddRedis();

            #endregion

            #region [Services]

            var vpc = new Vpc(this, "MyVpc", new VpcProps
            {
                MaxAzs = 2
            });

            var cluster = new Cluster(this, "MyCluster", new ClusterProps
            {
                Vpc = vpc,
            });

            AddAnalyzerApi(props, cluster, images);
            AddServerSlotApi(props, cluster, images);
            AddAuthenticationApi(props, cluster, images);
            AddServerMonitorApi(props, cluster, images);
            AddApiGateway(props, cluster, images);

            #endregion
        }

        #region [Infrastructure]

        public void AddKafka() { }
        public void AddDb() { }
        public void AddRedis() { }

        #endregion

        #region [Services]

        public void AddAuthenticationApi(ServerPulseStackProps props, Cluster cluster, DockerImages images)
        {
            var environment = new Dictionary<string, string>
            {
                { EnvironmentVariableKeys.AspCoreEnvironment, props.AspCoreEnvironment },
                { EnvironmentVariableKeys.ConnectionStringsAuthenticationDb, props.ConnectionStringsAuthenticationDb },
                { EnvironmentVariableKeys.ConnectionStringsRedisServer, props.ConnectionStringsRedisServer },
                { EnvironmentVariableKeys.EFCreateDatabase, props.EFCreateDatabase.ToString() },
                { EnvironmentVariableKeys.AuthSettingsPublicKey, props.AuthSettingsPublicKey },
                { EnvironmentVariableKeys.AuthSettingsPrivateKey, props.AuthSettingsPrivateKey },
                { EnvironmentVariableKeys.AuthSettingsAudience, props.AuthSettingsAudience },
                { EnvironmentVariableKeys.AuthSettingsIssuer, props.AuthSettingsIssuer },
                { EnvironmentVariableKeys.AuthSettingsExpiryInMinutes, props.AuthSettingsExpiryInMinutes.ToString() },
                { EnvironmentVariableKeys.AuthSettingsRefreshExpiryInDays, props.AuthSettingsRefreshExpiryInDays.ToString() },
                { EnvironmentVariableKeys.FeatureManagementOAuth, props.FeatureManagementOAuth.ToString() },
                { EnvironmentVariableKeys.FeatureManagementEmailConfirmation, props.FeatureManagementEmailConfirmation.ToString() },
                { EnvironmentVariableKeys.EmailConnectionString, props.EmailConnectionString },
                { EnvironmentVariableKeys.EmailSenderAddress, props.EmailSenderAddress },
                { EnvironmentVariableKeys.EmailAccessKey, props.EmailAccessKey },
                { EnvironmentVariableKeys.EmailSecretKey, props.EmailSecretKey },
                { EnvironmentVariableKeys.EmailRegion, props.EmailRegion },
                { EnvironmentVariableKeys.BackgroundServicesUseUserUnconfirmedCleanup, props.BackgroundServicesUseUserUnconfirmedCleanup.ToString() },
                { EnvironmentVariableKeys.BackgroundServicesUnconfirmedUsersCleanUpInMinutes, props.BackgroundServicesUnconfirmedUsersCleanUpInMinutes.ToString() }
            };

            var fargateService = new ApplicationLoadBalancedFargateService(this, "MyFargateAuthenticationApi", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(images.AuthenticationApi),
                    ContainerPort = 8080,
                    Environment = environment,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "MyFargateAuthenticationApiLogs"
                    })
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
            });

            fargateService.TaskDefinition.TaskRole.AddManagedPolicy(
                ManagedPolicy.FromAwsManagedPolicyName("AmazonEC2ContainerRegistryReadOnly"));
        }

        public void AddServerMonitorApi(ServerPulseStackProps props, Cluster cluster, DockerImages images)
        {
            var environment = new Dictionary<string, string>
            {
                { EnvironmentVariableKeys.AspCoreEnvironment, props.AspCoreEnvironment },
                { EnvironmentVariableKeys.KafkaBootstrapServers, props.KafkaBootstrapServers },
                { EnvironmentVariableKeys.KafkaClientIdMonitorApi, props.KafkaClientIdMonitorApi },
                { EnvironmentVariableKeys.MessageBusAliveTopic, props.MessageBusAliveTopic },
                { EnvironmentVariableKeys.MessageBusConfigurationTopic, props.MessageBusConfigurationTopic },
                { EnvironmentVariableKeys.MessageBusLoadTopic, props.MessageBusLoadTopic },
                { EnvironmentVariableKeys.MessageBusLoadTopicProcess, props.MessageBusLoadTopicProcess },
                { EnvironmentVariableKeys.MessageBusCustomTopic, props.MessageBusCustomTopic },
                { EnvironmentVariableKeys.ServerSlotApiUrl, props.ServerSlotApiUrl }
            };

            var fargateService = new ApplicationLoadBalancedFargateService(this, "MyServerMonitorApi", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(images.ServerMonitorApi),
                    ContainerPort = 8080,
                    Environment = environment,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "MyServerMonitorApiLogs"
                    })
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
            });

            fargateService.TaskDefinition.TaskRole.AddManagedPolicy(
                ManagedPolicy.FromAwsManagedPolicyName("AmazonEC2ContainerRegistryReadOnly"));
        }

        public void AddAnalyzerApi(ServerPulseStackProps props, Cluster cluster, DockerImages images)
        {
            var environment = new Dictionary<string, string>
            {
                { EnvironmentVariableKeys.AspCoreEnvironment, props.AspCoreEnvironment },
                { EnvironmentVariableKeys.KafkaBootstrapServers, props.KafkaBootstrapServers },
                { EnvironmentVariableKeys.KafkaClientIdAnalyzer, props.KafkaClientIdAnalyzer },
                { EnvironmentVariableKeys.KafkaGroupId, props.KafkaGroupId },
                { EnvironmentVariableKeys.MessageBusReceiveTimeoutInMilliseconds, props.MessageBusReceiveTimeoutInMilliseconds.ToString() },
                { EnvironmentVariableKeys.MessageBusAliveTopic, props.MessageBusAliveTopic },
                { EnvironmentVariableKeys.MessageBusConfigurationTopic, props.MessageBusConfigurationTopic },
                { EnvironmentVariableKeys.MessageBusLoadTopic, props.MessageBusLoadTopic },
                { EnvironmentVariableKeys.MessageBusCustomTopic, props.MessageBusCustomTopic },
                { EnvironmentVariableKeys.MessageBusLoadMethodStatisticsTopic, props.MessageBusLoadMethodStatisticsTopic },
                { EnvironmentVariableKeys.MessageBusLoadTopicProcess, props.MessageBusLoadTopicProcess },
                { EnvironmentVariableKeys.MessageBusServerStatisticsTopic, props.MessageBusServerStatisticsTopic },
                { EnvironmentVariableKeys.MessageBusTopicDataSaveInDays, props.MessageBusTopicDataSaveInDays.ToString() },
                { EnvironmentVariableKeys.PulseEventIntervalInMilliseconds, props.PulseEventIntervalInMilliseconds.ToString() },
                { EnvironmentVariableKeys.StatisticsCollectIntervalInMilliseconds, props.StatisticsCollectIntervalInMilliseconds.ToString() },
                { EnvironmentVariableKeys.ConnectionStringsRedisServer, props.ConnectionStringsRedisServer },
                { EnvironmentVariableKeys.MinimumStatisticsTimeSpanInSeconds, props.MinimumStatisticsTimeSpanInSeconds.ToString() },
                { EnvironmentVariableKeys.MaxEventAmountToGetInSlotData, props.MaxEventAmountToGetInSlotData.ToString() },
                { EnvironmentVariableKeys.MaxEventAmountToReadPerRequest, props.MaxEventAmountToReadPerRequest.ToString() },
                { EnvironmentVariableKeys.LoadEventProcessingBatchSize, props.LoadEventProcessingBatchSize.ToString() },
                { EnvironmentVariableKeys.LoadEventProcessingBatchIntervalInMilliseconds, props.LoadEventProcessingBatchIntervalInMilliseconds.ToString() }
            };

            var fargateService = new ApplicationLoadBalancedFargateService(this, "MyAnalyzerApi", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(images.AnalyzerApi),
                    ContainerPort = 8080,
                    Environment = environment,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "MyAnalyzerApiLogs"
                    })
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
            });

            fargateService.TaskDefinition.TaskRole.AddManagedPolicy(
                ManagedPolicy.FromAwsManagedPolicyName("AmazonEC2ContainerRegistryReadOnly"));

            props.ApiGateway = $"http://{fargateService.LoadBalancer.LoadBalancerDnsName}";
        }

        public void AddServerSlotApi(ServerPulseStackProps props, Cluster cluster, DockerImages images)
        {
            var environment = new Dictionary<string, string>
            {
                { EnvironmentVariableKeys.AspCoreEnvironment, props.AspCoreEnvironment },
                { EnvironmentVariableKeys.ConnectionStringsServerSlotDb, props.ConnectionStringsServerSlotDb },
                { EnvironmentVariableKeys.EFCreateDatabase, props.EFCreateDatabase.ToString() },
                { EnvironmentVariableKeys.SlotsPerUser, props.SlotsPerUser.ToString() },
                { EnvironmentVariableKeys.AuthSettingsPublicKey, props.AuthSettingsPublicKey },
                { EnvironmentVariableKeys.AuthSettingsAudience, props.AuthSettingsAudience },
                { EnvironmentVariableKeys.AuthSettingsIssuer, props.AuthSettingsIssuer },
                { EnvironmentVariableKeys.AuthSettingsExpiryInMinutes, props.AuthSettingsExpiryInMinutes.ToString() },
                { EnvironmentVariableKeys.ConnectionStringsRedisServer, props.ConnectionStringsRedisServer },
                { EnvironmentVariableKeys.CacheGetServerSlotByEmailExpiryInSeconds, props.CacheGetServerSlotByEmailExpiryInSeconds.ToString() },
                { EnvironmentVariableKeys.CacheServerSlotCheckExpiryInSeconds, props.CacheServerSlotCheckExpiryInSeconds.ToString() }
            };

            var fargateService = new ApplicationLoadBalancedFargateService(this, "MyServerSlotApi", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(images.ServerSlotApi),
                    ContainerPort = 8080,
                    Environment = environment,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "MyServerSlotApiLogs"
                    })
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
            });

            fargateService.TaskDefinition.TaskRole.AddManagedPolicy(
                ManagedPolicy.FromAwsManagedPolicyName("AmazonEC2ContainerRegistryReadOnly"));

            props.ServerSlotApiUrl = $"http://{fargateService.LoadBalancer.LoadBalancerDnsName}";
        }

        public void AddApiGateway(ServerPulseStackProps props, Cluster cluster, DockerImages images)
        {
            var environment = new Dictionary<string, string>
            {
                { EnvironmentVariableKeys.AspCoreEnvironment, props.AspCoreEnvironment },
                { EnvironmentVariableKeys.AuthSettingsPublicKey, props.AuthSettingsPublicKey },
                { EnvironmentVariableKeys.AuthSettingsAudience, props.AuthSettingsAudience },
                { EnvironmentVariableKeys.AuthSettingsIssuer, props.AuthSettingsIssuer },
                { EnvironmentVariableKeys.AuthSettingsExpiryInMinutes, props.AuthSettingsExpiryInMinutes.ToString() },
                { EnvironmentVariableKeys.AllowedCORSOrigins, props.AllowedCORSOrigins },
                { EnvironmentVariableKeys.UseCORS, props.UseCORS.ToString() }
            };

            var fargateService = new ApplicationLoadBalancedFargateService(this, "MyApiGateway", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(images.ApiGateway),
                    ContainerPort = 8080,
                    Environment = environment,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "MyApiGatewayLogs"
                    })
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
                AssignPublicIp = true,
                PublicLoadBalancer = true,
                TaskSubnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC },
            });

            var cfnService = fargateService.Service.Node.DefaultChild as CfnService;
            if (cfnService != null)
            {
                cfnService.AddPropertyOverride("NetworkConfiguration.AwsvpcConfiguration.AssignPublicIp", "ENABLED");
            }

            fargateService.TaskDefinition.TaskRole.AddManagedPolicy(
                ManagedPolicy.FromAwsManagedPolicyName("AmazonEC2ContainerRegistryReadOnly"));

            fargateService.TargetGroup.ConfigureHealthCheck(new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
            {
                Path = "/health",
                Interval = Duration.Seconds(30),
                Timeout = Duration.Seconds(5),
                HealthyThresholdCount = 2,
                UnhealthyThresholdCount = 3
            });

            fargateService.Service.Connections.SecurityGroups[0].AddIngressRule(
                Peer.AnyIpv4(),
                Port.Tcp(8080),
                "Allow public HTTP access over IPv4"
            );

            fargateService.Service.Connections.SecurityGroups[0].AddIngressRule(
                Peer.AnyIpv6(),
                Port.Tcp(8080),
                "Allow public HTTP access over IPv6"
            );
        }

        #endregion
    }
}