﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using main.Data;
using main.Models;

namespace main.Services
{
    public class OfferDataService : IOfferDataService
    {
        private readonly ApplicationDbContext _dbContext;
        public OfferDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Offer>> GetAllOffers()
        {
            return Task.FromResult(_dbContext.Offers.ToList());
        }

        public async Task<Offer> GetOffer(long id)
        {
            return await _dbContext.Offers.FindAsync(id);
        }

        public Task<Offer> GetOffer(Offer offer)
        {
            throw new NotImplementedException();
        }

        public Task<List<Offer>> GetOffersByCategory(string category)
        {
            throw new NotImplementedException();
        }

        public Task<List<Offer>> GetOffersByPublishDateInterval(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public Task<List<Offer>> GetOffersByType(string type)
        {
            throw new NotImplementedException();
        }

        public void UpdateOffer(long id, Offer offer)
        {
            throw new NotImplementedException();
        }

        public async void UpdateOffer(Offer offer)
        {
            _dbContext.Offers.Update(offer);
            await _dbContext.SaveChangesAsync();
        }

        public async void DeleteOffer(long id)
        {
            var offer = await _dbContext.Offers.FindAsync(id);
            if (offer.Applicants != null)
            {
                foreach (var applicant in offer.Applicants)
                {
                    _dbContext.Applicants.Remove(applicant);
                    await _dbContext.SaveChangesAsync();
                }
            }
            
            _dbContext.Offers.Remove(offer);
            await _dbContext.SaveChangesAsync();
        }

        public async void Save(Offer offer)
        {
            await _dbContext.Offers.AddAsync(offer);
            await _dbContext.SaveChangesAsync();
        }

        public Task<List<Offer>> GetOffersByUser(string userId)
        {
            return Task.FromResult(
                _dbContext.Offers.Where(o => o.Author == userId).ToList()
            );
        }

        public Task<List<Offer>> GetOffersByUser(string userId, int startIndex, int count)
        {
            return Task.FromResult(
                _dbContext.Offers
                    .Where(o => o.Author == userId)
                    .OrderByDescending(o => o.PublishDate).
                    Skip(startIndex).Take(count).ToList()
                );
        }

        public bool SaveWithResult(Offer offer)
        {
            try
            {
                _dbContext.Offers.Add(offer);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public Task<List<Offer>> GetOffersForHome(User user, int startIndex, int count)
        {
            var byUserPreference = _dbContext.Offers
                .Where(o => User.AreasOfExpertiseString.Contains(o.Category))
                .Where(o => o.Applicants.All(a => a.User.Id != user.Id))
                //.Where(o => o.Author != user.Id)
                .OrderByDescending(o => o.PublishDate)
                .Skip(startIndex).Take(count).ToList();
            if(byUserPreference.Count > 0)
            {
                return Task.FromResult(byUserPreference);
            }
            return Task.FromResult(
                _dbContext.Offers
                    //.Where(o => o.Author != user.Id)
                    .Where(o => o.Applicants.All(a => a.User.Id != user.Id))
                    .OrderByDescending(o => o.PublishDate)
                    .Skip(startIndex).Take(count).ToList()
                );
        }

        public bool AddApplicant(Offer offer, Applicant applicant)
        {
            try
            {
                _dbContext.Entry(offer).State = EntityState.Modified;
                _dbContext.Entry(applicant.User).State = EntityState.Modified;
                offer.Applicants ??= new List<Applicant>();
                offer.Applicants.Add(applicant);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool RemoveApplicant(Offer offer, Applicant applciant)
        {
            try
            {
                offer.Applicants.Remove(applciant);
                _dbContext.Offers.Update(offer);
                _dbContext.SaveChanges();
                _dbContext.Applicants.Remove(applciant);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool AcceptApplicant(Offer offer, Applicant applciant)
        {
            try
            {
                applciant.IsAccepted = true;
                _dbContext.Applicants.Update(applciant);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
