﻿using DENMAP_SERVER.Service;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DENMAP_SERVER.Entity;
using DENMAP_SERVER.Entity.dto;
using Nancy.ModelBinding;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using DENMAP_SERVER.Controller.request;


namespace DENMAP_SERVER.Controller
{
    public class CommentController : NancyModule
    {
        private UserService _userService = new UserService();
        private PostService _postService = new PostService();
        private CommentService _commentService = new CommentService();

        private readonly string _basePath = "/api/v1/comment";

        public CommentController()
        {

            Get(_basePath + "/", args =>
            {
                int? id = (int?)this.Request.Query["postId"];

                if (!id.HasValue)
                    return Response.AsJson(new { message = "Missing id parameter" }, HttpStatusCode.BadRequest);

                try
                {
                    Console.WriteLine("id: " + id.Value);
                    List<Comment> comments = _commentService.GetCommentsByPostId(id.Value);
                    Console.WriteLine("comments: " + comments.Count);

                    List<User> users = new List<User>();
                    if (comments.Count != 0)
                        users = _userService.GetUsersByIds(comments.Select(x => x.UserId).ToList());

                    Console.WriteLine("users: " + users.Count);
                    Dictionary<int, User> userDict = new Dictionary<int, User>();
                    comments.ForEach(x => userDict.Add(x.UserId, users.Find(y => y.Id == x.UserId)));
                    Console.WriteLine("userDict: " + userDict.Count);
                    List<CommentDTO> commentsDTO = new List<CommentDTO>();
                    Console.WriteLine("commentsDTO: " + commentsDTO.Count);
                    foreach (Comment comment in comments)
                        commentsDTO.Add(new CommentDTO(comment, userDict[comment.UserId]));
                    Console.WriteLine("commentsDTO: " + commentsDTO.Count);
                    return Response.AsJson(commentsDTO);

                }
                catch (Exception ex)
                {
                    return Response.AsJson(new { message = ex.Message }, HttpStatusCode.InternalServerError);
                }
            });

            Post(_basePath + "/", args =>
            {

                CommentRequest request = null;

                try
                {
                   request = this.Bind<CommentRequest>();
                }
                catch (Exception e)
                {
                    return Response.AsJson(new { message = e.Message }, HttpStatusCode.BadRequest);
                }

                Post post = null;
                try
                {
                    post = _postService.GetPostById(request.postId);
                    if (post == null)
                        return Response.AsJson(new { message = "Post not found" }, HttpStatusCode.NotFound);

                    User user = _userService.GetUserById(request.userId);
                    if (user == null)
                        return Response.AsJson(new { message = "User not found" }, HttpStatusCode.NotFound);
                }
                catch (Exception e)
                {
                    return Response.AsJson(new { message = e.Message }, HttpStatusCode.BadRequest);
                }


                try
                {
                    int commentId = _commentService.AddComment(request.userId, request.rating, request.message, request.postId);

                    _postService.ReCalculatePostRating(request.postId);
                    _userService.ReCalculateUserRating(post.UserId);
                    return Response.AsJson(new { message = request.postId }, HttpStatusCode.Created);
                }
                catch (Exception e)
                {
                    return Response.AsJson(new { message = e.Message }, HttpStatusCode.BadRequest);
                }
            });
        }
    }
}