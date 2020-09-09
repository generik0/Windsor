// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


namespace Castle.Windsor.Extensions.DependencyInjection
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	
	using Castle.Windsor;
	using Castle.Windsor.Extensions.DependencyInjection.Scope;

	using Microsoft.Extensions.DependencyInjection;

	internal class WindsorScopedServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
	{
		public ExtensionContainerScope parentScope { get; }
		public ExtensionContainerScope scope { get; }
		private bool disposing = false;

		private readonly IWindsorContainer container;
		
		public WindsorScopedServiceProvider(IWindsorContainer container, ExtensionContainerScope currentScope = null, ExtensionContainerScope parentScope = null)
		{
			this.container = container;
			if (currentScope != null)
			{
				this.scope = currentScope;
			}
			if (parentScope != null)
			{
				this.parentScope = parentScope;
			}
			if (this.scope == null)
			{
				scope = ExtensionContainerRootScope.RootScope;
			}
		}

		public object GetService(Type serviceType)
		{
			using(var fs = new ExtensionContainerScope.ForcedScope(scope, parentScope))
			{
				return ResolveInstanceOrNull(serviceType, true);	
			}
		}

		public object GetRequiredService(Type serviceType)
		{
			using(var fs = new ExtensionContainerScope.ForcedScope(scope, parentScope))
			{
				return ResolveInstanceOrNull(serviceType, false);	
			}
		}

		public void Dispose()
		{
			if(scope is ExtensionContainerRootScope)
			{
				if(!disposing)
				{
					disposing = true;
					var disposableScope = scope as IDisposable;
					if(disposableScope != null)
					{
						disposableScope.Dispose();
					}
					container.Dispose();
				}
				
			}
		}
		private object ResolveInstanceOrNull(Type serviceType, bool isOptional)
		{
			if (container.Kernel.HasComponent(serviceType))
			{
				return container.Resolve(serviceType);
			}

			if (serviceType.GetTypeInfo().IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				var allObjects = container.ResolveAll(serviceType.GenericTypeArguments[0]);
				return allObjects;
			}

			if (isOptional)
			{
				return null;
			}
			return container.Resolve(serviceType);
		}
	}
}