﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Tests.InvalidActions;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Tests.Services
{
    public class HandlerExistenceCheckerTests
    {
        [Fact]
        public void Verify_RegisteredAssemblyWithValidActions_DoesNotThrowExceptions()
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.AddActionsFromAssemblyOf<NopMessage>();
                c.AddHandlersFromAssemblyOf<NopMesageHandler>();
            });
            var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true});
        }

        [Fact]
        public void Verify_MessageWithoutHandler_ThrowExceptions()
        {
            ShouldThrow(MediatorExecutionException.CreateForNoHandler(typeof(MessageWithoutHandler)).Message);
        }

        [Fact]
        public void Verify_RequestWithoutHandler_ThrowExceptions()
        {
            ShouldThrow(MediatorExecutionException.CreateForNoHandler(typeof(RequestWithoutHandler)).Message);
        }

        private void ShouldThrow(string expectedError)
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.AddActionsFromAssemblyOf<MessageWithoutHandler>();
                c.AddHandlersFromAssemblyOf<MessageWithoutHandler>();
            });
            var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
            var ex = Assert.Throws<MediatorException>(() =>
            {
                sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
            });

            Assert.Contains(expectedError, ex.Message);
        }

        [Fact]
        public void Verify_ActionsWithoutHandlerThrowException()
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.AddActions(new Type[] { typeof(InvalidActionWithoutHandler) });
            });
            var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
            var ex = Assert.Throws<MediatorException>(() =>
            {
                sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
            });
            Assert.Equal(MediatorExecutionException.CreateForNoHandler(typeof(InvalidActionWithoutHandler)).Message, ex.Data["Error:1"].ToString());
        }

        [Fact]
        public void Verify_ActionsWithoutHandlerButWithNoHandlerAttribute_Pass()
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.AddActions(new Type[] { typeof(ValidActionWithoutHandler) });
            });
            var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
            sut.Verify(new ExistenceCheckerSetting { });
        }
    }
}
