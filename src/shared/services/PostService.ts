import { api, baseURL } from "./API";
import ResponseData from "../models/ResponseData";
import Page from "../models/Page";
import PostDto from "../dtos/PostDto";
import Post from "../models/Post";

export async function createPost(blogId: number, postDto: PostDto): Promise<ResponseData<Post>> {

    const token = localStorage.getItem('token');
    const response = await api.post(`/post/${blogId}`, postDto,
        {
            headers: {
                'Authorization': `${token}`,
                'Content-Type': 'multipart/form-data'
            }
        }
    )

    return response.data

}

export async function patchPost(postId: number, postDto: PostDto): Promise<ResponseData<Post>> {

    const token = localStorage.getItem('token');
    const response = await api.patch(`/post/${postId}`, postDto,
        {
            headers: {
                'Authorization': `${token}`,
                'Content-Type': 'multipart/form-data'
            }
        }
    )

    return response.data

}

export async function getUserPost(postId: number): Promise<ResponseData<Post>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/post/of-user/${postId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data

}

export async function getUserPostsByBlog(blogId: number, page = 1, size = 30): Promise<Page<Post>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/post/of-user/blog-posts/${blogId}?page=${page}&size=${size}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        })
        
    return response.data

}

export async function findUserPostByBlog(postTitle: string, blogId: number, page = 1, size = 30): Promise<Page<Post>> {
    
    const token = localStorage.getItem('token');
    const response = await api.get(`/post/of-user/blog-posts/${blogId}?search=${postTitle}&page=${page}&size=${size}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )
      
    return response.data

}

export async function deletePost(postId: number): Promise<void> {

    const token = localStorage.getItem('token');
    await api.delete(`/post/${postId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

}

export async function deleteCoverPost(postId: number): Promise<void> {

    const token = localStorage.getItem('token');
    await api.delete(`/post/delete-cover-post/${postId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

}

export async function getPublicPostsByBlog(blogId: number, page = 1, size = 30): Promise<Page<Post>> {
    const response = await api.get(`/post/public/blog-posts/${blogId}?page=${page}&size=${size}`)
    return response.data
}

export async function getPublicPosts(page = 1, size = 30): Promise<Page<Post>> {  
    const response = await api.get(`/post/public?page=${page}&size=${size}`)
    return response.data
}

export async function findPublicPost(postTitleSearch: string, page = 1, size = 30): Promise<Page<Post>> {  
    const response = await api.get(`/post/public?search=${postTitleSearch}&page=${page}&size=${size}`)
    return response.data
}

export async function findPublicPostByBlog(postTitleSearch: string, blogId: number, page = 1, size = 30): Promise<Page<Post>> {  
    const response = await api.get(`/post/public/blog-posts/${blogId}?search=${postTitleSearch}&page=${page}&size=${size}`)    
    return response.data
}

export async function getPublicPost(postId: number): Promise<ResponseData<Post>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/post/public/${postId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data

}

export async function getCoverImage(coverFileName: string): Promise<File> {

    const token = localStorage.getItem('token');
    const headers = {
        headers: {
            'Authorization': `${token}`
        }
    }
    
    /* const response = await api.get(`/post/cover-picture/${coverFileName}`, headers) */
 
    const response = await fetch(baseURL + `/post/cover-picture/${coverFileName}`, headers)
    const responseBlob = await response.blob()
  
    return new File([responseBlob], coverFileName)

}

