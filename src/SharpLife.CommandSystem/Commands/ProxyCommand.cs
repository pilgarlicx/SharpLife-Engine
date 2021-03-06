﻿/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using SharpLife.CommandSystem.TypeProxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpLife.CommandSystem.Commands
{
    /// <summary>
    /// A command that acts as a proxy for a delegate
    /// Arguments are automatically converted to the target type
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    internal sealed class ProxyCommand<TDelegate> : Command
        where TDelegate : Delegate
    {
        private readonly TDelegate _delegate;

        private readonly ITypeProxy[] _typeProxies;

        private readonly int _defaultValueCount;

        public ProxyCommand(CommandContext commandContext, string name,
            IReadOnlyList<CommandExecutor> executors,
            TDelegate @delegate,
            CommandFlags flags = CommandFlags.None, string helpInfo = "",
            object tag = null)
            : base(commandContext, name, executors, flags, helpInfo, tag)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));

            _typeProxies = _delegate.Method.GetParameters().Select(GetProxy).ToArray();

            //Determine how many arguments have default values
            _defaultValueCount = _delegate.Method.GetParameters().Count(p => p.HasDefaultValue);
        }

        private ITypeProxy GetProxy(ParameterInfo info)
        {
            var customProxy = info.GetCustomAttribute<TypeProxyAttribute>();

            if (customProxy != null)
            {
                var type = customProxy.Type;

                var interfaceType = typeof(ITypeProxy<>).MakeGenericType(info.ParameterType);

                if (!interfaceType.IsAssignableFrom(type))
                {
                    throw new ArgumentException($"Custom type proxy {type.FullName} must implement ITypeProxy<{info.ParameterType.FullName}>");
                }

                return _commandContext._commandSystem.GetParameterTypeProxy(type);
            }

            return _commandContext._commandSystem.GetTypeProxy(info.ParameterType);
        }

        private void ExecuteProxy(ICommandArgs command)
        {
            //Resolve each argument and attempt to convert it
            var parameters = _delegate.Method.GetParameters();
            var argumentCount = parameters.Length;
            var minimumArgumentCount = argumentCount - _defaultValueCount;

            if (command.Count < minimumArgumentCount)
            {
                _commandContext._logger.Information("Not enough arguments for proxy command {Name}: at least {ExpectedCount} expected (maximum {MaximumCount}, got {ReceivedCount}", Name, minimumArgumentCount, argumentCount, command.Count);
                return;
            }
            else if (command.Count > argumentCount)
            {
                _commandContext._logger.Information("Too many arguments for proxy command {Name}: {ExpectedCount} expected, got {ReceivedCount}", Name, argumentCount, command.Count);
                return;
            }

            var arguments = argumentCount > 0 ? new object[argumentCount] : null;

            int i;

            for (i = 0; i < command.Count; ++i)
            {
                var proxy = _typeProxies[i];

                if (!proxy.TryParse(command[i], _commandContext._commandSystem._provider, out var result))
                {
                    _commandContext._logger.Information("Proxy command {Name}: could not convert argument {Index} to type {Type}", Name, i, parameters[i].ParameterType.Name);
                    return;
                }

                arguments[i] = result;
            }

            //Set any default arguments
            for (; i < argumentCount; ++i)
            {
                arguments[i] = parameters[i].DefaultValue;
            }

            //TODO: this is a bit overkill, need to decide on how this needs to be handled
            try
            {
                _delegate.DynamicInvoke(arguments);
            }
            catch (Exception e)
            {
                _commandContext._logger.Error(e, "An error occurred while executing a proxy command");
            }
        }

        internal override void OnCommand(ICommandArgs command)
        {
            //Execute the proxy before the other handlers to maintain consistency with non-proxy commands
            ExecuteProxy(command);

            base.OnCommand(command);
        }

        public override string ToString()
        {
            return $"Proxy command {Name}({string.Join(", ", _delegate.Method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}" + (p.HasDefaultValue ? $" = {p.DefaultValue}" : "")))})";
        }
    }
}
