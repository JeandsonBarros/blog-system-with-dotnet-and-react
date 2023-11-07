import { Button, Progress } from "@nextui-org/react";
import { useContext, useEffect, useState } from "react";
import Post from "../../shared/models/Post";
import { MainContext } from "../../App";
import { axiosErrorToString } from "../../shared/services/API";
import { getPublicPosts } from "../../shared/services/PostService";
import { MdExpandMore, MdRateReview } from "react-icons/md";
import { Link } from "react-router-dom";
import PostCard from "../../components/PostCard";

export default function Home() {

    const { setAlert } = useContext(MainContext)
    const [posts, setPosts] = useState<Post[]>([])
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [pagination, setPagination] = useState<any>({ totalPages: 1, actualPage: 1, totalRecords: 0 })

    useEffect(() => {
        getListPosts()
    }, [])

    async function getListPosts(page = 1): Promise<void> {

        setIsLoad(true)

        try {

            const response = await getPublicPosts(page)
            setPosts(page == 1 ? response.data : posts.concat(response.data))

            setPagination({
                totalPages: response.totalPages,
                actualPage: response.page,
                totalRecords: response.totalRecords
            })

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    return (
        <section className="max-w-7xl m-auto p-3">

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            <h1 className="text-2xl mt-2"> Most recent post </h1>

            <hr />

            <div className="flex flex-col w-full">

                <div className="flex flex-row flex-wrap">
                    {posts.map(post => (
                        <div key={post.id} className="m-3">

                            <Link
                                to={`/blog/${post.blog.id}/${post.blog.title.toLowerCase().replace(/\W/g, "-")}`}
                                className="flex flex-row items-center mb-2 hover:font-medium h-4"
                            >
                                <MdRateReview className="me-1" /> {post.blog.title}
                            </Link>

                            <PostCard
                                post={post}
                                blogTitle={post.blog.title}
                            />

                        </div>
                    ))}
                </div>

                {(posts.length > 0 && posts.length < pagination.totalRecords) &&
                    <Button
                        title="Show more"
                        variant="light"
                        color="default"
                        onPress={() => getListPosts(pagination.actualPage + 1)}
                    >
                        <MdExpandMore className="text-xl" />
                    </Button>
                }

            </div>

        </section>
    );
}
