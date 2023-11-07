import { useContext, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import FormPost from "../../components/FormPost";
import PostDto from "../../shared/dtos/PostDto";
import { axiosErrorToString } from "../../shared/services/API";
import { getUserPost, patchPost } from "../../shared/services/PostService";
import Post from "../../shared/models/Post";
import { Progress } from "@nextui-org/react";
import { MainContext } from "../../App";

export default function UpdatePost() {

    const { setAlert } = useContext(MainContext)
    const params = useParams()
    const navigate = useNavigate()
    const [post, setPost] = useState<Post>()
    const [isLoad, setIsLoad] = useState<boolean>(false)

    useEffect(() => {
        getPostData()
    }, [])

    async function getPostData() {
        setIsLoad(true)
        try {
            const response = await getUserPost(Number(params.postId))
            setPost(response.data)
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }
        setIsLoad(false)
    }

    async function updatePost(postDto: PostDto): Promise<void> {
        setIsLoad(true)
        try {
            await patchPost(Number(params.postId), postDto)
            setAlert({ text: "Update successfully.", status: "success", isVisible: true })
            navigate(`/user-blogs`)
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }
        setIsLoad(false)
    }

    return (
        <section className="p-3 m-auto flex flex-col justify-center items-center max-w-7xl">

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            <h1 className="text-2xl w-full">Update Post</h1>

            <hr className="w-full" />

            <FormPost
                action={updatePost}
                postUpdate={post}
            />

        </section>
    );
}
