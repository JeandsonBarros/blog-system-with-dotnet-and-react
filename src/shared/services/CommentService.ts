import { api } from "./API";
import Page from "../models/Page";
import Comment from "../models/Comment";

export async function getCommentsPost(postId: number, page = 1, size = 30): Promise<Page<Comment>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/comment/post-comments/${postId}?page=${page}&size=${size}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        })
        
    return response.data
}

export async function postComment(postId: number, commentText: string) {

    const token = localStorage.getItem('token');
    const response = await api.post(`/comment/${postId}`, { commentText },
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data
}

export async function putComment(commentId: number, commentText: string) {

    const token = localStorage.getItem('token');
    const response = await api.put(`/comment/${commentId}`, { commentText },
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data
}

export async function deleteComment(commentId: number) {

    const token = localStorage.getItem('token');
    const response = await api.delete(`/comment/${commentId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data
}