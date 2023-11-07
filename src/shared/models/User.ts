interface Role {
    id: number
    roleName: string
}

export default interface User {
    id: number,
    name: string
    email: string
    fileProfilePictureName?: string
    roles: Role[]
}

