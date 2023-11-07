export default interface ResponseData<T>{
    message?: string
    date: string
    success: boolean
    details?: string
    data: T
}