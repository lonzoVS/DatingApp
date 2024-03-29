using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinariConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinariConfig)
        {
            _cloudinariConfig = cloudinariConfig;
            _mapper = mapper;
            _repo = repo;
            Account acc = new Account(_cloudinariConfig.Value.CloudName,
            _cloudinariConfig.Value.ApiKey,
            _cloudinariConfig.Value.ApiSecret);

            _cloudinary = new Cloudinary(acc);
            
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo =  _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhoroForCretionDto photoForCretionDto)
        {
            if (userId != int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId, true);

            var file = photoForCretionDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill")
                        .Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCretionDto.Url = uploadResult.Uri.ToString();
            photoForCretionDto.PublicId = uploadResult.PublicId;
            var photo = _mapper.Map<Photo>(photoForCretionDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            userFromRepo.Photos.Add(photo);
            
            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo); 
                return CreatedAtRoute("GetPhoto", new { id = photo.Id}, photoToReturn);
            }

            return BadRequest("Couldn't add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var userFromRepo = await _repo.GetUser(userId, true);

            if (!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("This photo is already  set as main!");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent();
            
            return BadRequest("Couldn't set photo to main");
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhtot(int userId, int id)
        {
             if (userId != int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var userFromRepo = await _repo.GetUser(userId, true);

            if (!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("You can't delete your main photo!");

           
            if (photoFromRepo.PublicId != null)
            {
                 var result = _cloudinary.Destroy(new DeletionParams(photoFromRepo.PublicId));

                if (result.Result == "ok")
                    _repo.Delete(photoFromRepo);

                
            }
            else
                _repo.Delete(photoFromRepo);
            
            if (await _repo.SaveAll())
                    return Ok();
     
            return BadRequest("Failed to deelte the photo");
        }

    }
}