import { Link } from "react-router-dom"
import Post from "../shared/models/Post"
import ImgExample from "../assets/img/img_example.png"
import { MdChatBubble } from "react-icons/md"
import { useEffect, useState } from "react"
import { getCommentsPost } from "../shared/services/CommentService"
import { getCoverImage } from "../shared/services/PostService"

export default function PostCard({ post, blogTitle, classStyles }: { post: Post, blogTitle: string, classStyles?: string }) {

    const [quantityComments, setQuantityComments] = useState<number>(0)
    const [imgCover, setImgCover] = useState<string>(ImgExample)

    useEffect(() => {

        getCommentsPost(post.id)
            .then(response => setQuantityComments(response.totalRecords))

        if (post.coverFileName) {
            getCoverImage(post.coverFileName)
                .then(file => setImgCover(URL.createObjectURL(file)))
        }

    }, [post])

    return (
        <Link
            to={`/blog/${post.blogId}/${blogTitle.toLowerCase().replace(/\W/g, "-")}/${post.id}/${post.title.toLowerCase().replace(/\W/g, "-")}`}
            className={`block rounded-lg border border-gray-300 shadow-md hover:shadow-lg w-80 ${classStyles}`}
        >

            <img
                src={imgCover}
                className={`w-80 h-64 object-cover`}
            />

            <div className="p-2">

                <h1 className="text-2xl">{post.title}</h1>

                <p className="my-1">{post.subtitle}</p>

                <small className="text-slate-500">
                    In {(() => (new Date(post.date).toLocaleDateString("en-US")))()}
                </small>

                <p className="flex flex-row items-center my-1">
                    <MdChatBubble className="me-1" />
                    {quantityComments}
                </p>

            </div>

        </Link>
    )
}