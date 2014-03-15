﻿using AutoMapper;
using System.Linq;

namespace WebSignalR.Infrastructure
{
	internal static class AutomapperExtnsions
	{
		public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
		{
			var sourceType = typeof(TSource);
			var destinationType = typeof(TDestination);
			var existingMaps = Mapper.GetAllTypeMaps().First(x => x.SourceType.Equals(sourceType)
				&& x.DestinationType.Equals(destinationType));
			foreach (var property in existingMaps.GetUnmappedPropertyNames())
			{
				expression.ForMember(property, opt => opt.Ignore());
			}
			return expression;
		}
	}
}