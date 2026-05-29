using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _service;

        public QueueController(
            IQueueService service)
        {
            _service = service;
        }

        // ==========================
        // ADD PATIENT TO QUEUE
        // ==========================
        [HttpPost("add")]
        public async Task<IActionResult>
            Add(
            QueueCreateDto dto) 
        {
            var result =
                await _service
                .AddToQueueAsync(dto);

            return Ok(result);
        }

        // ==========================
        // GET ALL QUEUE
        // ==========================
        [HttpGet("all")]
        public async Task<IActionResult>
            GetAll()
        {
            var result =
                await _service
                .GetAllQueueAsync();

            return Ok(result);
        }

        // ==========================
        // GET DOCTOR QUEUE
        // ==========================
        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult>
            GetDoctor(
            int doctorId)
        {
            var result =
                await _service
                .GetDoctorQueueAsync(
                    doctorId);

            return Ok(result);
        }

        // ==========================
        // GET BY STAGE
        // ==========================
        [HttpGet("stage/{stage}")]
        public async Task<IActionResult>
            GetStage(
            string stage)
        {
            var result =
                await _service
                .GetByStageAsync(
                    stage);

            return Ok(result);
        }

        // ==========================
        // CALL NEXT
        // ==========================
        [HttpPost("call-next/{doctorId}")]
        public async Task<IActionResult>
            CallNext(
            int doctorId)
        {
            var result =
                await _service
                .CallNextAsync(
                    doctorId);

            if (result == null)
                return Ok(new
                {
                    success = false,
                    message = "No patients waiting"
                });

            return Ok(result);
        }

        // ==========================
        // COMPLETE
        // ==========================
        [HttpPost("complete/{id}")]
        public async Task<IActionResult>
            Complete(
            int id)
        {
            var result =
                await _service
                .CompleteAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ==========================
        // SKIP
        // ==========================
        [HttpPost("skip/{id}")]
        public async Task<IActionResult>
            Skip(
            int id)
        {
            var result =
                await _service
                .SkipAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ==========================
        // MOVE STAGE
        // ==========================
        [HttpPost("move/{id}")]
        public async Task<IActionResult>
            Move(
            int id,
            [FromQuery]
            string stage)
        {
            var result =
                await _service
                .MoveStageAsync(
                    id,
                    stage);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ==========================
        // RESET QUEUE
        // ==========================
        [HttpDelete("reset")]
        public async Task<IActionResult>
            Reset()
        {
            var result =
                await _service
                .ResetQueueAsync();

            return Ok(new
            {
                success = true,
                message = result
            });
        }

        // ==========================
        [HttpPost("recall/{id}")]
        public async Task<IActionResult> Recall(
            int id)
        {
            var result =
                await _service.RecallAsync(id);

            if (result == null)
                return NotFound(
                    "Patient Not Found");

            return Ok(result);
        }


        //=====================
        [HttpGet("display/{displayNumber}")]
        public async Task<IActionResult>
        GetDisplayQueue(
        string displayNumber)
        {
            var data =
                await _service
                .GetDisplayQueueAsync(
                    displayNumber);

            return Ok(data);
        }


        //=======================CNETRAL DISPLAY
        [HttpGet("central-display")]
        public async Task<IActionResult>
        GetCentralDisplay()
            {
                var data =
                    await _service
                    .GetCentralDisplayAsync();

                return Ok(data);
            }


        //==========================
        [HttpPut("reset-doctor-queue/{doctorId}")]
        public async Task<IActionResult>
        ResetDoctorQueue(int doctorId)
        {
            var result = await _service.ResetDoctorQueueAsync(doctorId);

            return Ok(result);
        }

        [HttpPost("emergency/{id}")]
        public async Task<IActionResult> 
        Emergency(int id)
        {
            var result =
                await _service
                .EmergencyCallAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

    }
}