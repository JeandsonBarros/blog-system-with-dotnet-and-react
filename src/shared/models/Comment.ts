import Post from "./Post"
import User from "./User"

export default interface Comment{
    id: number
    commentText: string
    date: string
    isUpdated: boolean
    postId: number
    post?: Post
    user?: User
}