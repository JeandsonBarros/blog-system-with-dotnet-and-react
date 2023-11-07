export default interface Page<T> {
    data: T[]
    page: number
    size: number
    totalPages: number
    totalRecords: number
}
