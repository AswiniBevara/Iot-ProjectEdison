﻿using System;
using Edison.Common;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Edison.Common.Config;

namespace Edison.ChatService.Repositories
{
    public interface ICosmosDBRepositoryChat<T>
    {
        Task<T> GetItemAsync(string id);
        Task<T> GetItemAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetItemAsync(Expression<Func<T, bool>> where, Expression<Func<T, T>> select);
        Task<bool> IsItemExistsByIdAsync(string id);
        Task<bool> IsItemExistsByNonIdAsync(Expression<Func<T, bool>> where);
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> select);
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetItemsAsyncOrderByAscending(Expression<Func<T, bool>> where, Expression<Func<T, DateTime>> orderBy, int limit = 100);
        Task<IEnumerable<T>> GetItemsAsyncOrderByDescending(Expression<Func<T, bool>> where, Expression<Func<T, DateTime>> orderByDescending, int limit = 100);
        Task<CosmosDBPageResponse<T>> GetItemsPagingAsync(int pageSize, string continuationToken);
        Task<CosmosDBPageResponse<T>> GetItemsPagingAsync(Expression<Func<T, bool>> predicate, int pageSize, string continuationToken);
        Task<IEnumerable<T>> GetItemsAsync();
        Task<string> CreateItemAsync(T item);
        Task<string> CreateOrUpdateItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        Task<bool> DeleteItemsAsync(Expression<Func<T, bool>> where);
    }
}
