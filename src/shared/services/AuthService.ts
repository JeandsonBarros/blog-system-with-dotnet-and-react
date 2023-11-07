import { api, baseURL } from "./API";
import UserDto from '../dtos/UserDto';
import User from '../models/User';
import ResponseMessage from "../models/ResponseMessage";
import ResponseData from "../models/ResponseData";
import Page from "../models/Page";

export async function login(email: string, password: string): Promise<ResponseData<string>> {
    const response = await api.post('/auth/login', { email, password })
    localStorage.setItem('token', response.data.data)
    return response.data
}

export async function register(userDto: UserDto): Promise<ResponseData<User>> {

    const response = await api.post(
        '/auth/register',
        userDto,
        {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        }
    )

    return response.data
}

export async function getDataAccount(): Promise<ResponseData<User>> {

    const token = localStorage.getItem('token');
    const response = await api.get('/auth/account-data',
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return response.data

}

export async function patchAccount(userDTO: UserDto): Promise<ResponseData<User>> {

    const token = localStorage.getItem('token');
    const response = await api.patch('/auth/update-account', userDTO,
        {
            headers: {
                'Authorization': `${token}`,
                'Content-Type': 'multipart/form-data'
            }
        }
    )

    return response.data

}

export async function deleteAccount(): Promise<string> {

    const token = localStorage.getItem('token');
    await api.delete('/auth/delete-account',
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return "User deleted"

}

export async function sendEmailToForgottenPassword(email: string): Promise<ResponseMessage> {
    const response = await api.post(`/auth/forgotten-password/send-email-code`, { email })
    return response.data
}

export async function changeForgottenPassword(email: string, newPassword: string, code: number): Promise<ResponseData<User>> {
    const response = await api.put(`/auth/forgotten-password/change-password`, { email, newPassword, code })
    return response.data
}

export async function userDeleteProfilePhoto(): Promise<string> {

    const token = localStorage.getItem('token');
    await api.delete(`/auth/delete-profile-image`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return "Photo deleted successfully"

}

export async function getProfileImage(imgName: string): Promise<File> {

    const token = localStorage.getItem('token');
    const headers = {
        headers: {
            'Authorization': `${token}`
        }
    }
    
    /* const response = await api.get(`/post/cover-picture/${coverFileName}`, headers) */
 
    const response = await fetch(baseURL + `/auth/profile-picture/${imgName}`, headers)
    const responseBlob = await response.blob()
  
    return new File([responseBlob], imgName)

}

/* --------- Admin and Master functions ------------ */

export async function getUsers(page = 0, size = 30): Promise<Page<User>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/auth/list-users?page=${page}&size=${size}`,
        {
            headers: {
                'Authorization': `${token}`,
            }
        }
    )

    return response.data

}

export async function findUser(email: string, page = 0, size = 30): Promise<Page<User>> {

    const token = localStorage.getItem('token');
    const response = await api.get(`/auth/list-users?search=${email}&page=${page}&size=${size}`,
        {
            headers: {
                'Authorization': `${token}`,
            }
        }
    )

    return response.data

}

export async function patchAUser(userId: number, user: UserDto): Promise<ResponseData<User>> {

    const token = localStorage.getItem('token');
    const response = await api.patch(`/auth/update-a-user/${userId}`, user,
        {
            headers: {
                'Authorization': `${token}`,
                'Content-Type': 'multipart/form-data'
            }
        }
    )

    return response.data

}

export async function deleteAUser(userId: number): Promise<string> {


    const token = localStorage.getItem('token');
    await api.delete(`/auth/delete-a-user/${userId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return "User deleted successfully"

}

export async function deleteAUserProfilePhoto(userId: number): Promise<string> {


    const token = localStorage.getItem('token');
    await api.delete(`/auth/delete-a-user-photo/${userId}`,
        {
            headers: {
                'Authorization': `${token}`
            }
        }
    )

    return "Photo deleted successfully"

}