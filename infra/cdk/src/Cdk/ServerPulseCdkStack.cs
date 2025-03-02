using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElastiCache;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.ServiceDiscovery;
using Amazon.CDK.AWS.SSM;
using Constructs;
using System.Collections.Generic;
using System.Linq;
using CfnService = Amazon.CDK.AWS.ECS.CfnService;
using Cluster = Amazon.CDK.AWS.ECS.Cluster;
using ClusterProps = Amazon.CDK.AWS.ECS.ClusterProps;
using InstanceType = Amazon.CDK.AWS.EC2.InstanceType;

namespace Cdk
{
    public class ServerPulseCdkStack : Stack
    {
        internal ServerPulseCdkStack(Construct scope, string id, ServerPulseStackProps props, DockerImages images) : base(scope, id, props)
        {
            var vpc = new Vpc(this, "MyVpc", new VpcProps
            {
                MaxAzs = 2
            });

            var namespaceName = "server-pulse";
            var cluster = new Cluster(this, "MyCluster", new ClusterProps
            {
                Vpc = vpc,
                DefaultCloudMapNamespace = new CloudMapNamespaceOptions
                {
                    Name = namespaceName,
                    Type = NamespaceType.DNS_PRIVATE,
                    Vpc = vpc
                }
            });

            #region [Infrastructure]

            AddKafka(vpc, props);
            AddDb(vpc, props);
            AddRedis(vpc, props);

            #endregion

            #region [Services]

            var apigatewayName = "apigateway";
            var serverslotName = "serverslotapi";
            var analyzerName = "analyzerapi";
            var authenticationName = "authenticationapi";
            var servermonitorName = "servermonitorapi";

            props.ApiGateway = $"http://{apigatewayName}.{namespaceName}:8080";
            props.ServerSlotApiUrl = $"http://{serverslotName}.{namespaceName}:8080";

            AddApiGateway(props, cluster, images, apigatewayName);
            AddServerSlotApi(props, cluster, images, serverslotName);
            AddAnalyzerApi(props, cluster, images, analyzerName);
            AddAuthenticationApi(props, cluster, images, authenticationName);
            AddServerMonitorApi(props, cluster, images, servermonitorName);

            #endregion

            #region [Frontend]

            var frontendName = "frontend";

            AddFrontend(cluster, images, frontendName);

            #endregion
        }

        #region [Infrastructure]

        public void AddKafka(Vpc vpc, ServerPulseStackProps props)
        {
            var kafkaSecurityGroup = new SecurityGroup(this, "KafkaSecurityGroup", new SecurityGroupProps
            {
                Vpc = vpc,
                AllowAllOutbound = true
            });

            kafkaSecurityGroup.AddIngressRule(
                kafkaSecurityGroup,
                Port.Tcp(9092),
                "Allow internal Kafka broker communication"
            );

            var kafkaCluster = new Amazon.CDK.AWS.MSK.CfnCluster(this, "KafkaCluster", new Amazon.CDK.AWS.MSK.CfnClusterProps
            {
                ClusterName = "ServerPulseKafka",
                KafkaVersion = "3.3.1",
                NumberOfBrokerNodes = 2,
                BrokerNodeGroupInfo = new Amazon.CDK.AWS.MSK.CfnCluster.BrokerNodeGroupInfoProperty
                {
                    InstanceType = "kafka.t3.small",
                    SecurityGroups = [kafkaSecurityGroup.SecurityGroupId],
                    StorageInfo = new Amazon.CDK.AWS.MSK.CfnCluster.StorageInfoProperty
                    {
                        EbsStorageInfo = new Amazon.CDK.AWS.MSK.CfnCluster.EBSStorageInfoProperty
                        {
                            VolumeSize = 5
                        }
                    },
                    ClientSubnets = vpc.PrivateSubnets.Select(subnet => subnet.SubnetId).ToArray()
                },
                EnhancedMonitoring = "DEFAULT",  // Disables detailed monitoring (reduces cost)
                ConfigurationInfo = new Amazon.CDK.AWS.MSK.CfnCluster.ConfigurationInfoProperty
                {
                    Arn = "arn:aws:kafka:region:account-id:configuration/your-config",
                    Revision = 1
                }
            });

            kafkaCluster.ApplyRemovalPolicy(RemovalPolicy.DESTROY);

            var kafkaBootstrapParam = new StringParameter(this, "KafkaBootstrapServers", new StringParameterProps
            {
                ParameterName = "/serverpulse/kafka/bootstrap-servers",
                StringValue = Fn.GetAtt(kafkaCluster.LogicalId, "BootstrapBrokers").ToString(),
                Tier = ParameterTier.STANDARD
            });

            props.KafkaBootstrapServers = kafkaBootstrapParam.StringValue;
        }

        public void AddDb(Vpc vpc, ServerPulseStackProps props)
        {
            var dbSecurityGroup = new SecurityGroup(this, "PostgresSecurityGroup", new SecurityGroupProps
            {
                Vpc = vpc,
                AllowAllOutbound = true
            });

            dbSecurityGroup.AddIngressRule(
                dbSecurityGroup,
                Port.Tcp(5432),
                "Allow internal access to PostgreSQL"
            );

            //var dbCredentials = Credentials.FromGeneratedSecret("user1", new CredentialsBaseOptions
            //{
            //    SecretName = "serverpulse-db-secret"
            //});

            var dbInstance = new DatabaseInstance(this, "ServerPulsePostgres", new DatabaseInstanceProps
            {
                Engine = DatabaseInstanceEngine.Postgres(new PostgresInstanceEngineProps
                {
                    Version = PostgresEngineVersion.VER_14
                }),
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
                Vpc = vpc,
                SecurityGroups = [dbSecurityGroup],
                Credentials = Credentials.FromPassword(props.PostgresUser, new SecretValue(props.PostgresPassword)),
                MultiAz = false,
                AllocatedStorage = 20,
                StorageType = StorageType.GP2,
                StorageEncrypted = false,
                BackupRetention = Duration.Days(0),
                RemovalPolicy = RemovalPolicy.DESTROY,
                DeletionProtection = false,
            });


            var dbConnectionParam = new StringParameter(this, "DbConnectionString", new StringParameterProps
            {
                ParameterName = "/serverpulse/postgres/connection-string",
                StringValue = $"User ID={props.PostgresUser};Password={props.PostgresPassword};Host={dbInstance.InstanceEndpoint.Hostname};Port=5432;Database={props.PostgresDb};Pooling=true;MinPoolSize=0;MaxPoolSize=100;ConnectionLifetime=0;TrustServerCertificate=true",
                Tier = ParameterTier.STANDARD
            });

            dbSecurityGroup.AddIngressRule(
                Peer.Ipv4(vpc.VpcCidrBlock),
                Port.Tcp(5432),
                "Allow VPC-wide access to PostgreSQL Ipv4"
            );

            props.ConnectionStringsAuthenticationDb = dbConnectionParam.StringValue;
            props.ConnectionStringsServerSlotDb = dbConnectionParam.StringValue;
        }

        public void AddRedis(Vpc vpc, ServerPulseStackProps props)
        {
            var redisSecurityGroup = new SecurityGroup(this, "RedisSecurityGroup", new SecurityGroupProps
            {
                Vpc = vpc,
                AllowAllOutbound = true
            });

            redisSecurityGroup.AddIngressRule(
                redisSecurityGroup,
                Port.Tcp(6379),
                "Allow internal access to Redis"
            );

            var redisSubnetGroup = new CfnSubnetGroup(this, "RedisSubnetGroup", new CfnSubnetGroupProps
            {
                Description = "Subnet group for Redis cluster",
                SubnetIds = vpc.PrivateSubnets.Select(subnet => subnet.SubnetId).ToArray()
            });

            var redisCluster = new CfnCacheCluster(this, "RedisCluster", new CfnCacheClusterProps
            {
                ClusterName = "ServerPulseRedis",
                Engine = "redis",
                CacheNodeType = "cache.t4g.micro",
                NumCacheNodes = 1,
                VpcSecurityGroupIds = [redisSecurityGroup.SecurityGroupId],
                CacheSubnetGroupName = redisSubnetGroup.Ref,
                EngineVersion = "7.0",
                AutoMinorVersionUpgrade = false,
                SnapshotRetentionLimit = 0,
            });

            redisCluster.ApplyRemovalPolicy(RemovalPolicy.DESTROY);

            var redisConnectionParam = new StringParameter(this, "RedisConnectionString", new StringParameterProps
            {
                ParameterName = "/serverpulse/redis/connection-string",
                StringValue = $"redis://{redisCluster.AttrRedisEndpointAddress}:6379",
                Tier = ParameterTier.STANDARD
            });

            redisSecurityGroup.AddIngressRule(
                Peer.Ipv4(vpc.VpcCidrBlock),
                Port.Tcp(6379),
                "Allow VPC-wide access to Redis Ipv4"
            );

            props.ConnectionStringsRedisServer = redisConnectionParam.StringValue;
        }

        #endregion

        #region [Services]

        public void AddAuthenticationApi(ServerPulseStackProps props, Cluster cluster, DockerImages images, string serviceName)
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

            AddService(cluster, images.AuthenticationApi, serviceName, environment);
        }

        public void AddServerMonitorApi(ServerPulseStackProps props, Cluster cluster, DockerImages images, string serviceName)
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

            AddService(cluster, images.ServerMonitorApi, serviceName, environment);
        }

        public void AddAnalyzerApi(ServerPulseStackProps props, Cluster cluster, DockerImages images, string serviceName)
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

            AddService(cluster, images.AnalyzerApi, serviceName, environment);
        }

        public void AddServerSlotApi(ServerPulseStackProps props, Cluster cluster, DockerImages images, string serviceName)
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

            AddService(cluster, images.ServerSlotApi, serviceName, environment);
        }

        public void AddApiGateway(ServerPulseStackProps props, Cluster cluster, DockerImages images, string serviceName)
        {
            var environment = new Dictionary<string, string>
            {
                { EnvironmentVariableKeys.AspCoreEnvironment, "CDK" },
                { EnvironmentVariableKeys.AuthSettingsPublicKey, props.AuthSettingsPublicKey },
                { EnvironmentVariableKeys.AuthSettingsAudience, props.AuthSettingsAudience },
                { EnvironmentVariableKeys.AuthSettingsIssuer, props.AuthSettingsIssuer },
                { EnvironmentVariableKeys.AuthSettingsExpiryInMinutes, props.AuthSettingsExpiryInMinutes.ToString() },
                { EnvironmentVariableKeys.AllowedCORSOrigins, props.AllowedCORSOrigins },
                { EnvironmentVariableKeys.UseCORS, props.UseCORS.ToString() }
            };

            var fargateService = new ApplicationLoadBalancedFargateService(this, $"{serviceName}Service", new ApplicationLoadBalancedFargateServiceProps
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
                        StreamPrefix = $"{serviceName}ServiceLogs"
                    }),
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
                ServiceName = serviceName,
                CloudMapOptions = new CloudMapOptions
                {
                    Name = serviceName,
                    DnsRecordType = DnsRecordType.A,
                    DnsTtl = Duration.Seconds(60)
                },
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

        private void AddService(Cluster cluster, string dockerImage, string serviceName, Dictionary<string, string> environment)
        {
            var fargateService = new ApplicationLoadBalancedFargateService(this, $"{serviceName}Service", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(dockerImage),
                    ContainerPort = 8080,
                    Environment = environment,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = $"{serviceName}ServiceLogs"
                    }),
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
                ServiceName = serviceName,
                CloudMapOptions = new CloudMapOptions
                {
                    Name = serviceName,
                    DnsRecordType = DnsRecordType.A,
                    DnsTtl = Duration.Seconds(60)
                }
            });

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

        #region [Frontend]

        private void AddFrontend(Cluster cluster, DockerImages images, string serviceName)
        {
            var fargateService = new ApplicationLoadBalancedFargateService(this, $"{serviceName}Service", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(images.Frontend),
                    ContainerPort = 80,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = $"{serviceName}ServiceLogs"
                    }),
                },
                MemoryLimitMiB = 1024,
                Cpu = 512,
                ServiceName = serviceName,
                CloudMapOptions = new CloudMapOptions
                {
                    Name = serviceName,
                    DnsRecordType = DnsRecordType.A,
                    DnsTtl = Duration.Seconds(60)
                },
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

            fargateService.Service.Connections.SecurityGroups[0].AddIngressRule(
                Peer.AnyIpv4(),
                Port.Tcp(80),
                "Allow public HTTP access over IPv4"
            );

            fargateService.Service.Connections.SecurityGroups[0].AddIngressRule(
                Peer.AnyIpv6(),
                Port.Tcp(80),
                "Allow public HTTP access over IPv6"
            );
        }

        #endregion
    }
}