import { Button, Progress } from '@nextui-org/react';
import { useContext, useEffect, useState } from 'react';
import { MdExpandMore } from 'react-icons/md';
import { useLocation, useNavigate } from 'react-router-dom';

import { MainContext } from '../../App';
import PostCard from '../../components/PostCard';
import Post from '../../shared/models/Post';
import { axiosErrorToString } from '../../shared/services/API';
import { findPublicPost } from '../../shared/services/PostService';

export default function SearchPublicPost() {

    const { setAlert } = useContext(MainContext)
    const location = useLocation()
    const navigate = useNavigate()
    const [posts, setPosts] = useState<Post[]>([])
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [titleValueSearch, setTitleValueSearch] = useState<string>("")
    const [pagination, setPagination] = useState<any>({ totalPages: 1, actualPage: 1, totalRecords: 0 })

    useEffect(() => {
        listPosts()
    }, [location])

    async function listPosts(page = 1): Promise<void> {

        setIsLoad(true)

        try {

            const queryParams = new URLSearchParams(location.search);
            const titleValue = queryParams.get("title");

            if (!titleValue) {
                navigate(`/`)
                return
            }

            setTitleValueSearch(titleValue)

            let response = await findPublicPost(titleValue, page)
            page == 1 ? setPosts(response.data) : setPosts(posts.concat(response.data))

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
        <section className="p-3">

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            <h1 className="text-2xl mt-2">Search by: {titleValueSearch}</h1>

            <hr />

            <div className="flex flex-col w-full items-center w-2/3">

                <div className="flex flex-row flex-wrap justify-center" >
                    {posts.map(post => (
                        <PostCard
                            key={post.id}
                            post={post}
                            blogTitle={post.blog.title}
                            classStyles='m-3'
                        />
                    ))}
                </div>

                <div className="flex items-center">
                    {(posts.length > 0 && posts.length < pagination.totalRecords) &&
                        <Button
                            className="m-auto"
                            isIconOnly
                            title="Show more"
                            variant="light"
                            color="default"
                            radius="full"
                            onPress={() => listPosts(pagination.actualPage + 1)}
                        >
                            <MdExpandMore className="text-xl" />
                        </Button>
                    }
                </div>

            </div>

        </section>
    )
}