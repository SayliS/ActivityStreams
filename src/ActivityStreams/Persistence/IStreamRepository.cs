﻿using System.Linq;
using ActivityStreams.Helpers;

namespace ActivityStreams.Persistence
{
    public interface IStreamRepository
    {
        ActivityStream Load(byte[] streamId);

        void AttachStream(byte[] sourceStreamId, byte[] streamIdToAttach);

        void DetachStream(byte[] sourceStreamId, byte[] streamIdToDetach, long detachedSince);
    }

    public class DefaultStreamRepository : IStreamRepository
    {
        readonly IStreamStore store;

        public DefaultStreamRepository(IStreamStore store)
        {
            this.store = store;
        }

        public void AttachStream(byte[] sourceStreamId, byte[] streamIdToAttach)
        {
            store.Attach(sourceStreamId, streamIdToAttach, ActivityStream.DefaultExpirationTimestamp);
        }

        public void DetachStream(byte[] sourceStreamId, byte[] streamIdToDetach, long detachedSince)
        {
            store.Detach(sourceStreamId, streamIdToDetach, detachedSince);
        }

        public ActivityStream Load(byte[] streamId)
        {
            var result = store.Get(streamId);
            if (ReferenceEquals(null, result))
            {
                result = new ActivityStream(streamId);
                if (result.Attach(streamId).IsSuccessful)
                    AttachStream(streamId, streamId);
            }
            else
            {
                result.ExpiresAt = result.AttachedStreams.Single(x => ByteArrayHelper.Compare(x.StreamId, streamId)).ExpiresAt;
            }
            return result;
        }
    }
}