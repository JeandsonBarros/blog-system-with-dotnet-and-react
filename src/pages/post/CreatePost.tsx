import { useContext } from "react";
import FormPost from "../../components/FormPost";
import PostDto from "../../shared/dtos/PostDto";
import { axiosErrorToString } from "../../shared/services/API";
import { createPost } from "../../shared/services/PostService";
import { useNavigate, useParams } from "react-router-dom";
import { MainContext } from "../../App";

function CreatePost() {

    const params = useParams()
    const navigate = useNavigate()
    const { setAlert } = useContext(MainContext)

    async function createNewPost(postDto: PostDto): Promise<void> {
        try {
            await createPost(Number(params.blogId), postDto)
            setAlert({ text: "Create successfully.", status: "success", isVisible: true })
            navigate(`/user-blogs`)
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }
    }

    return (
        <section className="p-3 m-auto flex flex-col justify-center items-center max-w-7xl">

            <h1 className="text-2xl w-full">Create Post</h1>

            <hr className="w-full" />

            <FormPost action={createNewPost} />

        </section>
    );
}

export default CreatePost;