import User from "./User"

export default interface Blog {
    id: number
    title: string
    description: string
    headerColor?: string
    titleColor?: string
    isPublic: boolean
    userAuthId: number
    userAuth: User
}