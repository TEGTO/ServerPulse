﻿using EventCommunication.Events;
using MediatR;

namespace ServerMonitorApi.Command.SendLoadEvents
{
    public record SendLoadEventsCommand(LoadEvent[] Events) : IRequest<Unit>;
}