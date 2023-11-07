import { Button, Progress } from '@nextui-org/react';
import { useContext, useEffect, useState } from 'react';
import { MdExpandMore, MdInfo } from 'react-icons/md';
import { Link, useLocation, useParams } from 'react-router-dom';

import { MainContext } from '../../App';
import ImgExample from '../../assets/img/img_example.png';
import UserImgExample from '../../assets/img/person-circle.svg';
import PostCard from '../../components/PostCard';
import Blog from '../../shared/models/Blog';
import Post from '../../shared/models/Post';
import { axiosErrorToString, baseURL } from '../../shared/services/API';
import { getPublicBlog, getUserBlog } from '../../shared/services/BlogService';
import { getCoverImage, getPublicPostsByBlog, getUserPostsByBlog } from '../../shared/services/PostService';
import BlogViewStyles from '../../styles/pages_styles/blog_styles/blog_view.module.css';

export default function BlogView() {

    const { setAlert } = useContext(MainContext)
    const params = useParams()
    const location = useLocation()
    const [posts, setPosts] = useState<Post[]>([])
    const [blog, setBlog] = useState<Blog>()
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [pagination, setPagination] = useState<any>({ totalPages: 1, actualPage: 1, totalRecords: 0 })
    const [urlCoverPostHighlight, setUrlCoverPostHighlight] = useState<string>(ImgExample)
    const [isPreview, setIsPreview] = useState<boolean>(false)

    useEffect(() => {
        getBlogData()
        listPosts()
    }, [location])

    useEffect(() => {
        if (posts.length > 0 && posts[0].coverFileName) {
            getCoverImage(posts[0].coverFileName)
                .then(file => setUrlCoverPostHighlight(URL.createObjectURL(file)))
        }
    }, [posts])

    async function getBlogData() {

        setIsLoad(true)

        try {

            const pathnameSplit = location.pathname.split("/")
            const isPreviewTemp = pathnameSplit[pathnameSplit.length - 1] == "preview" && pathnameSplit.length == 5
            const response = isPreviewTemp
                ? await getUserBlog(Number(params.blogId))
                : await getPublicBlog(Number(params.blogId))

            setBlog(response.data)
            setIsPreview(isPreviewTemp)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)
    }

    async function listPosts(page = 1): Promise<void> {

        setIsLoad(true)

        try {

            const pathnameSplit = location.pathname.split("/")
            const isPreview = pathnameSplit[pathnameSplit.length - 1] == "preview" && pathnameSplit.length == 5
            const response = isPreview
                ? await getUserPostsByBlog(Number(params.blogId), page)
                : await getPublicPostsByBlog(Number(params.blogId), page)

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
        <section className="max-w-7xl m-auto">

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            {isPreview &&
                <p className='flex flex-row items-center ps-2 text-xl h-10 bg-sky-600 text-white z-40 w-full '>
                    <MdInfo className="me-1" /> Blog Preview
                </p>
            }

            {(!isLoad && posts.length == 0) &&
                <p className="mt-3">Nem uma postagem encontrada.</p>
            }

            {posts.length > 0 &&
                <Link
                    to={`/blog/${posts[0].blogId}/${blog?.title.toLowerCase().replace(/\W/g, "-")}/${posts[0].id}/${posts[0].title.toLowerCase().replace(/\W/g, "-")}`}
                    className="flex flex-row items-center flex-wrap-reverse justify-center border-y-2 border-gray-300"
                >

                    <div className="m-2 w-96">

                        <h1 className="text-5xl"><i>{posts[0].title}</i></h1>

                        <p>
                            {posts[0].subtitle.slice(0, 100)}
                        </p>

                        <p className="text-slate-500">
                            In {(() => (new Date(posts[0].date).toLocaleDateString("en-US")))()}
                        </p>

                    </div>

                    <img className="w-96 h-80 object-cover" src={urlCoverPostHighlight} />

                </Link>
            }

            <div className="flex flex-row justify-center">

                <div className="flex flex-col w-full items-center w-2/3">

                    <div className="flex flex-row flex-wrap justify-center" >
                        {posts.slice(1).map(post => (
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

                <div className={`w-1/3 h-fit mt-5 rounded-lg ${BlogViewStyles.aboutBlog}`}>
                    {(blog && blog.userAuth) &&
                        <>
                            <div className="text-center m-5">
                                <h1 className="text-xl">About</h1>
                                <p>{blog.description}</p>
                            </div>

                            <hr />

                            <div className="flex flex-row items-center">

                                <img
                                    className="w-16 h-16 rounded-full object-cover m-3"
                                    src={blog.userAuth.fileProfilePictureName
                                        ? `${baseURL}/auth/profile-picture/${blog.userAuth.fileProfilePictureName}`
                                        : UserImgExample
                                    }
                                />

                                <p >{blog.userAuth.name}</p>

                            </div>
                        </>
                    }
                </div>

            </div>

        </section>
    );
}
