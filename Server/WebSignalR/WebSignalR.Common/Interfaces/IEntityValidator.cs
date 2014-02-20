﻿using System;
using System.Collections.Generic;

namespace WebSignalR.Common.Interfaces
{
	/// <summary>
	/// The entity validator base contract
	/// </summary>
	public interface IEntityValidator
	{
		/// <summary>
		/// Perform validation and return if the entity state is valid
		/// </summary>
		/// <typeparam name="TEntity">The type of entity to validate</typeparam>
		/// <param name="item">The instance to validate</param>
		/// <returns>True if entity state is valid</returns>
		bool IsValid<TEntity>(TEntity item)
			where TEntity : Entities.EntityBase;

		/// <summary>
		/// Return the collection of errors if entity state is not valid
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="item">The instance with validation errors</param>
		/// <returns>A collection of validation errors</returns>
		IEnumerable<String> GetInvalidMessages<TEntity>(TEntity item)
			where TEntity : Entities.EntityBase;
	}

	/// <summary>
	/// Base contract for entity validator abstract factory
	/// </summary>
	public interface IEntityValidatorFactory
	{
		/// <summary>
		/// Create a new IEntityValidator
		/// </summary>
		/// <returns>IEntityValidator</returns>
		IEntityValidator Create();
	}
}