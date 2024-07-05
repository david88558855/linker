﻿using linker.client;
using linker.config;
using linker.server;
using linker.libs;
using MemoryPack;

namespace linker.plugins.signin.messenger
{
    public sealed class SignInClientMessenger : IMessenger
    {
        private readonly ConfigWrap config;
        private readonly ClientSignInTransfer clientSignInTransfer;
        public SignInClientMessenger(ConfigWrap config, ClientSignInTransfer clientSignInTransfer)
        {
            this.config = config;
            this.clientSignInTransfer = clientSignInTransfer;
        }

        [MessengerId((ushort)SignInMessengerIds.Name)]
        public void Name(IConnection connection)
        {
            ConfigSetNameInfo info = MemoryPackSerializer.Deserialize<ConfigSetNameInfo>(connection.ReceiveRequestWrap.Payload.Span);
            clientSignInTransfer.UpdateName(info.NewName);
        }

    }

    public sealed class SignInServerMessenger : IMessenger
    {
        private readonly SignCaching signCaching;
        private readonly ConfigWrap config;
        private readonly MessengerSender messengerSender;

        public SignInServerMessenger(SignCaching signCaching, ConfigWrap config, MessengerSender messengerSender)
        {
            this.signCaching = signCaching;
            this.config = config;
            this.messengerSender = messengerSender;
        }

        [MessengerId((ushort)SignInMessengerIds.SignIn)]
        public void SignIn(IConnection connection)
        {
            SignInfo info = MemoryPackSerializer.Deserialize<SignInfo>(connection.ReceiveRequestWrap.Payload.Span);
            if (info.Version == config.Data.Version)
            {
                signCaching.Sign(connection, info);
                connection.Write(MemoryPackSerializer.Serialize(info.MachineId));
            }
            else
            {
                connection.Write(Helper.FalseArray);
            }
        }


        [MessengerId((ushort)SignInMessengerIds.List)]
        public void List(IConnection connection)
        {
            SignInListRequestInfo request = MemoryPackSerializer.Deserialize<SignInListRequestInfo>(connection.ReceiveRequestWrap.Payload.Span);

            if (signCaching.TryGet(connection.Id, out SignCacheInfo cache))
            {
                IEnumerable<SignCacheInfo> list = signCaching.Get(cache.GroupId).OrderByDescending(c => c.MachineName).OrderByDescending(c => c.LastSignIn).OrderByDescending(c => c.Version).ToList();
                if (string.IsNullOrWhiteSpace(request.Name) == false)
                {
                    list = list.Where(c => c.MachineName.Contains(request.Name));
                }
                int count = list.Count();
                list = list.Skip((request.Page - 1) * request.Size).Take(request.Size);

                SignInListResponseInfo response = new SignInListResponseInfo { Request = request, Count = count, List = list.ToList() };

                connection.Write(MemoryPackSerializer.Serialize(response));
            }
        }

        [MessengerId((ushort)SignInMessengerIds.Delete)]
        public void Delete(IConnection connection)
        {
            string name = MemoryPackSerializer.Deserialize<string>(connection.ReceiveRequestWrap.Payload.Span);
            if (signCaching.TryGet(name, out SignCacheInfo cache) && signCaching.TryGet(connection.Id, out SignCacheInfo cache1) && cache.GroupId == cache1.GroupId)
            {
                signCaching.TryRemove(name, out _);
            }
        }

        [MessengerId((ushort)SignInMessengerIds.NameForward)]
        public async Task NameForward(IConnection connection)
        {
            ConfigSetNameInfo info = MemoryPackSerializer.Deserialize<ConfigSetNameInfo>(connection.ReceiveRequestWrap.Payload.Span);
            if (signCaching.TryGet(info.Id, out SignCacheInfo cache) && signCaching.TryGet(connection.Id, out SignCacheInfo cache1) && cache.GroupId == cache1.GroupId)
            {
                if (info.Id != connection.Id)
                {
                    await messengerSender.SendOnly(new MessageRequestWrap
                    {
                        Connection = cache.Connection,
                        MessengerId = (ushort)SignInMessengerIds.Name,
                        Payload = connection.ReceiveRequestWrap.Payload,
                    }).ConfigureAwait(false);
                }
            }
        }

        [MessengerId((ushort)SignInMessengerIds.Version)]
        public void Version(IConnection connection)
        {
            connection.Write(MemoryPackSerializer.Serialize(config.Data.Version));
        }

    }

    [MemoryPackable]
    public sealed partial class SignInListRequestInfo
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int Page { get; set; } = 1;
        /// <summary>
        /// 每页大小
        /// </summary>
        public int Size { get; set; } = 10;
        /// <summary>
        /// 所在分组
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 按名称搜索
        /// </summary>
        public string Name { get; set; }
    }

    [MemoryPackable]
    public sealed partial class SignInListResponseInfo
    {
        public SignInListRequestInfo Request { get; set; } = new SignInListRequestInfo();
        public int Count { get; set; }
        public List<SignCacheInfo> List { get; set; } = new List<SignCacheInfo>();
    }
}
