using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using FuelStationWebApi.Models;
using FuelStationWebApi.Data;
using Microsoft.EntityFrameworkCore;
using FuelStationWebApi.ViewModels;

namespace FuelStationWebApi.Controllers
{
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
        private readonly FuelsContext _context;
        public OperationsController(FuelsContext context)
        {
            _context = context;
        }

        // GET api/values
        [HttpGet]
        [Produces("application/json")]
        public List<OperationViewModel> Get()
        {
            var ovm = _context.Operations.Include(t => t.Tank).Include(f => f.Fuel).Select(o=> 
                new OperationViewModel
                {
                    OperationID=o.OperationID,
                    TankID=o.TankID,
                    FuelID=o.FuelID,
                    FuelType=o.Fuel.FuelType,
                    TankType=o.Tank.TankType,
                    Inc_Exp=o.Inc_Exp,
                    Date=o.Date

                });
            return ovm.OrderByDescending(t=>t.OperationID).Take(20).ToList();
        }
        // GET api/values
        [HttpGet("fuels")]
        [Produces("application/json")]
        public IEnumerable<Fuel> GetFuels()
        {
            return _context.Fuels.ToList();
        }
        // GET api/values
        [HttpGet("tanks")]
        [Produces("application/json")]
        public IEnumerable<Tank> GetTanks()
        {
            return _context.Tanks.ToList();
        }



        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Operation operation = _context.Operations.FirstOrDefault(x => x.OperationID == id);
            if (operation == null)
                return NotFound();
            return new ObjectResult(operation);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]Operation operation)
        {
            if (operation == null)
            {
                return BadRequest();
            }

            _context.Operations.Add(operation);
            _context.SaveChanges();
            return Ok(operation);
        }

        // PUT api/values/5
        [HttpPut]
        public IActionResult Put([FromBody]Operation operation)
        {
            if (operation == null)
            {
                return BadRequest();
            }
            if (!_context.Operations.Any(x => x.OperationID == operation.OperationID))
            {
                return NotFound();
            }

            _context.Update(operation);
            _context.SaveChanges();


            return Ok(operation);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Operation operation = _context.Operations.FirstOrDefault(x => x.OperationID == id);
            if (operation == null)
            {
                return NotFound();
            }
            _context.Operations.Remove(operation);
            _context.SaveChanges();
            return Ok(operation);
        }
    }
}
