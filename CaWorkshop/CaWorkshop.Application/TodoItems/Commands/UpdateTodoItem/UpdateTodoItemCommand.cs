﻿using CaWorkshop.Application.Common.GuardClauses;
using CaWorkshop.Application.Common.Interfaces;
using CaWorkshop.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CaWorkshop.Application.Common.Security;

namespace CaWorkshop.Application.TodoItems.Commands.UpdateTodoItem
{
    [Authorise]
    public partial class UpdateTodoItemCommand : IRequest
    {
        public long Id { get; set; }

        public int ListId { get; set; }

        public string Title { get; set; }

        public bool Done { get; set; }

        public int Priority { get; set; }

        public string Note { get; set; }
    }

    public class UpdateTodoItemCommandHandler
            : IRequestHandler<UpdateTodoItemCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTodoItemCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateTodoItemCommand request,
                CancellationToken cancellationToken)
        {
            var entity = await _context.TodoItems.FindAsync(request.Id);

            Guard.Against.NotFound(entity, nameof(entity), request.Id);

            // should not use automapper reversemap for command, better to see what changes for write operation. 
            entity.ListId = request.ListId;
            entity.Title = request.Title;
            entity.Done = request.Done;
            entity.Priority = (PriorityLevel)request.Priority;
            entity.Note = request.Note;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
