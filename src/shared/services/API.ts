import axios from "axios";

export const baseURL = "http://localhost:8080/api";

export const api = axios.create({
    baseURL: baseURL,
});

export function axiosErrorToString(error: any): string {

    if (axios.isAxiosError(error)) {

        if (error.response?.data?.message) {
            return error.response.data.message
        } else if (error.response?.data?.errors) {

            return Object.keys(error.response.data.errors).map((item) => {
                return error?.response?.data.errors[item][0]
            })[0];

        }

    }

    return "There was an error when making the request.";
}