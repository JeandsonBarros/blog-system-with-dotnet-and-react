import { api } from "./API";
import ResponseData from "../models/ResponseData";
import Page from "../models/Page";
import BlogDto from "../dtos/BlogDto";
import Blog from "../models/Blog";

export async function getUserBlogs(page = 1, size = 30): Promise<Page<Blog>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/blog/of-user?page=${page}&size=${size}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data
}

export async function getUserBlog(blogId: number): Promise<ResponseData<Blog>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/blog/of-user/${blogId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data

}

export async function getPublicBlog(blogId: number): Promise<ResponseData<Blog>> {
    const response = await api.get(`/blog/public/${blogId}`)
    return response.data
}

export async function findUserBlogByTitle(blogTitle: string, page = 1, size = 30): Promise<Page<Blog>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/blog/of-user?search=${blogTitle}&page=${page}&size=${size}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data
}

export async function findPublicBlogByName(blogTitle: string, page = 1, size = 30): Promise<Page<Blog>> {
    const response = await api.get(`/blog/public?search=${blogTitle}&page=${page}&size=${size}`)
    return response.data
}

export async function postBlog(blogDto: BlogDto): Promise<ResponseData<Blog>> {

    const token = localStorage.getItem('token');
    const response = await api.post('/blog', blogDto,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data

}

export async function patchBlog(blogId: number, blogDto: BlogDto): Promise<ResponseData<Blog>> {

    const token = localStorage.getItem('token');
    const response = await api.patch(`/blog/${blogId}`, blogDto,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data

}

export async function deleteBlog(blogId: number): Promise<void> {

    const token = localStorage.getItem('token');
    await api.delete(`/blog/${blogId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

}