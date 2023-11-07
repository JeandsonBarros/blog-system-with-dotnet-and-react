import Blog from "./Blog"

export default interface Post{
    id: number
    title: string
    subtitle: string
    coverFileName: string
    text: string
    date: string
    isUpdated: boolean
    isPublic: boolean
    userAuthId: number
    blogId: number
    blog: Blog
}