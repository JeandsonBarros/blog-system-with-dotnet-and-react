import {
    Button,
    ButtonGroup,
    Dropdown,
    DropdownItem,
    DropdownMenu,
    DropdownTrigger,
    Input,
    Pagination,
    Progress,
} from '@nextui-org/react';
import { useContext, useEffect, useRef, useState } from 'react';
import { MdAddCircle, MdDelete, MdEditSquare, MdExpandMore, MdMenu, MdOutlineMoreVert, MdVisibility } from 'react-icons/md';
import { Link, useNavigate } from 'react-router-dom';

import { MainContext } from '../../App';
import ModalBlog from '../../components/BlogModal';
import BlogDto from '../../shared/dtos/BlogDto';
import Blog from '../../shared/models/Blog';
import Post from '../../shared/models/Post';
import { axiosErrorToString } from '../../shared/services/API';
import { deleteBlog, findUserBlogByTitle, getUserBlogs, patchBlog, postBlog } from '../../shared/services/BlogService';
import { deletePost, findUserPostByBlog, getUserPostsByBlog } from '../../shared/services/PostService';
import UserBlogsStyles from '../../styles/pages_styles/blog_styles/user_blogs.module.css';

export default function UserBlogs() {

    const [blogs, setBlogs] = useState<Blog[]>([]);
    const [selectedBlog, setSelectedBlog] = useState<Blog>()
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [isOpen, setIsOpen] = useState<boolean>(false)
    const [pagination, setPagination] = useState<any>({ totalPages: 1, actualPage: 1, totalRecords: 0 })
    const [searchBlog, setSearchBlog] = useState<string>()
    const navBlogsRef = useRef<HTMLInputElement>(null)
    const { setAlert } = useContext(MainContext)

    useEffect(() => {
        listBlogs()
    }, [searchBlog])

    async function listBlogs(page = 1): Promise<void> {

        setIsLoad(true)

        try {

            const response = searchBlog ? await findUserBlogByTitle(searchBlog, page) : await getUserBlogs(page)
            page == 1 ? setBlogs(response.data) : setBlogs(blogs.concat(response.data))

            setPagination({
                totalPages: response.totalPages,
                actualPage: response.page,
                totalRecords: response.totalRecords
            })

            if (response.data.length > 0 && page == 1) setSelectedBlog(response.data[0])

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)
    }

    async function createBlog(blogDto: BlogDto): Promise<void> {

        try {

            const response = await postBlog(blogDto)
            setAlert({ text: "Created successfully.", status: "success", isVisible: true })

            const tempBlogs = [...blogs]
            tempBlogs.push(response.data)
            setBlogs(tempBlogs)

            setSelectedBlog(response.data)

            setIsOpen(false)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

    }

    return (
        <section>

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            <div className="flex flex-row items-center m-3">

                <Button
                    className={`text-2xl ${UserBlogsStyles.btnNavBlogs}`}
                    isIconOnly
                    variant="light"
                    onPress={() => {
                        if (navBlogsRef?.current?.style) {
                            navBlogsRef.current.style.display == "block"
                                ? navBlogsRef.current.style.display = "none"
                                : navBlogsRef.current.style.display = "block"
                        }
                    }}
                >
                    <MdMenu />
                </Button>

                <h1 className="text-2xl"> Your blogs </h1>

            </div>

            <hr />

            <div className="flex flex-row ">

                <nav
                    ref={navBlogsRef}
                    className={`bg-white z-50 w-52 border-r-1 ${UserBlogsStyles.navBlogs}`}
                >

                    <Input
                        variant="underlined"
                        placeholder="Find blog"
                        onValueChange={setSearchBlog}
                    />

                    <Button
                        variant="flat"
                        color="primary"
                        radius="none"
                        className="w-full"
                        onPress={() => setIsOpen(true)}
                    >
                        <MdAddCircle />  New blog
                    </Button>

                    <div className="overflow-auto h-screen">
                        {blogs.map(blog => (
                            <Button
                                key={blog.id}
                                className="w-full"
                                variant={selectedBlog && selectedBlog.title == blog.title ? "solid" : "light"}
                                radius="none"
                                onPress={() => setSelectedBlog(blog)}
                            >
                                {blog.title}
                            </Button>
                        ))}

                        {(blogs.length > 0 && blogs.length < pagination.totalRecords) &&
                            <Button
                                title="Show more"
                                variant="light"
                                color="default"
                                radius="none"
                                className="w-full"
                                onPress={() => listBlogs(pagination.actualPage + 1)}
                            >
                                <MdExpandMore className="text-xl" />
                            </Button>
                        }

                    </div>

                </nav>

                {selectedBlog &&
                    <BlogManagerView
                        selectedBlog={selectedBlog}
                        setSelectedBlog={setSelectedBlog}
                        setBlogs={setBlogs}
                        blogs={blogs}
                    />
                }

            </div>

            {(!isLoad && blogs.length == 0) &&
                <div className="flex justify-center mt-3">
                    <p>Not a blog found</p>
                </div>
            }

            {/* Modal add blog */}
            <ModalBlog
                isOpen={isOpen}
                setIsOpen={setIsOpen}
                action={createBlog}
            />

        </section>
    );
}

interface BlogManagerViewProps {
    selectedBlog: Blog
    setSelectedBlog: (blog: Blog) => void
    blogs: Blog[]
    setBlogs: (blogs: Blog[]) => void
}
function BlogManagerView({ selectedBlog, setSelectedBlog, blogs, setBlogs }: BlogManagerViewProps) {

    const navigate = useNavigate()
    const [isOpen, setIsOpen] = useState<boolean>(false)
    const [posts, setPosts] = useState<Post[]>([])
    const [pagination, setPagination] = useState<any>({ totalPages: 1, actualPage: 1 })
    const [searchPost, setSearchPost] = useState<string>()
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const { setAlert } = useContext(MainContext)

    useEffect(() => {
        listPosts()
    }, [selectedBlog, searchPost])

    async function listPosts(page = 1): Promise<void> {

        setIsLoad(true)

        try {
            const response = searchPost
                ? await findUserPostByBlog(searchPost, selectedBlog.id, page)
                : await getUserPostsByBlog(selectedBlog.id, page)
            setPosts(response.data)
            setPagination({ totalPages: response.totalPages, actualPage: response.page })
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisibility: true })
        }

        setIsLoad(false)

    }

    async function updateBlog(blogDto: BlogDto): Promise<void> {
        try {

            const response = await patchBlog(selectedBlog.id, blogDto)
            setSelectedBlog(response.data)
            setAlert({ text: "Updated successfully.", status: "success", isVisibility: true })

            const index = blogs.map(blogItem => blogItem.id).indexOf(selectedBlog.id);
            blogs[index] = response.data
            setBlogs([...blogs])

            setIsOpen(false)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisibility: true })
        }
    }

    async function removeBlog(): Promise<void> {
        try {

            await deleteBlog(selectedBlog.id)
            setAlert({ text: "Deleted successfully.", status: "success", isVisibility: true })

            const index = blogs.map(blogItem => blogItem.id).indexOf(selectedBlog.id);
            blogs.splice(index, 1);
            setBlogs([...blogs])

            if (blogs.length > 0) setSelectedBlog(blogs[0])

            setIsOpen(false)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisibility: true })
        }
    }

    async function removePost(postId: number): Promise<void> {
        try {

            await deletePost(postId)
            setAlert({ text: "Deleted successfully.", status: "success", isVisibility: true })

            const index = posts.map(postItem => postItem.id).indexOf(postId);
            posts.splice(index, 1);
            setPosts([...posts])

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisibility: true })
        }
    }

    return (
        <div className="w-full">

            <div className="flex flex-row flex-wrap justify-between items-center m-3">

                <h1 className="text-2xl"> {selectedBlog.title} </h1>

                <Input
                    className="w-40"
                    variant="underlined"
                    placeholder="Find post"
                    onValueChange={setSearchPost}
                />

            </div>

            <ButtonGroup className="m-3">
                <Button
                    isIconOnly
                    color="primary"
                    title={`Go to ${selectedBlog.title}`}
                    onPress={() => {
                        navigate(`/blog/${selectedBlog.id}/${selectedBlog.title.replace(/\W/g, "-").toLowerCase()}/preview`)
                    }}
                >
                    <MdVisibility />
                </Button>
                <Button
                    isIconOnly
                    title="New post"
                    color="secondary"
                    onPress={() => navigate(`/blog/${selectedBlog.id}/${selectedBlog.title.replace(/\W/g, "-").toLowerCase()}/create-post`)}
                >
                    <MdAddCircle />
                </Button>
                <Button
                    isIconOnly
                    title="Update blog"
                    color="warning"
                    onPress={() => setIsOpen(true)}
                >
                    <MdEditSquare />
                </Button>
                <Button
                    isIconOnly
                    title="Delete blog"
                    color="danger"
                    onPress={removeBlog}
                >
                    <MdDelete />
                </Button>
            </ButtonGroup>

            <hr />

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            <div className="overflow-auto h-screen">

                {
                    posts.map(post => (
                        <div className="flex flex-row justify-between items-center p-2 border-b-1" key={post.id}>

                            <Link
                                to={`/blog/${selectedBlog.id}/${selectedBlog.title.replace(/\W/g, "-").toLowerCase()}/${post.id}/${post.title.replace(/\W/g, "-").toLowerCase()}/preview`}
                                title={`Go to ${post.title} `}
                            >

                                <p> {post.title}</p>

                                <small className="text-slate-500">
                                    In {(() => (new Date(post.date).toLocaleDateString("en-US")))()}
                                </small>

                            </Link>

                            <Dropdown>
                                <DropdownTrigger>
                                    <Button
                                        isIconOnly
                                        variant="light"
                                        aria-label="Like"
                                    >
                                        <MdOutlineMoreVert />
                                    </Button>
                                </DropdownTrigger>
                                <DropdownMenu aria-label="Static Actions">
                                    <DropdownItem
                                        key="update"
                                        onPress={() => navigate(`/blog/${selectedBlog.id}/${selectedBlog.title.replace(/\W/g, "-").toLowerCase()}/update-post/${post.id}`)}
                                    >
                                        Update
                                    </DropdownItem>
                                    <DropdownItem
                                        key="delete"
                                        className="text-danger"
                                        color="danger"
                                        onPress={() => removePost(post.id)}
                                    >
                                        Delete
                                    </DropdownItem>
                                </DropdownMenu>
                            </Dropdown>

                        </div>
                    ))
                }

                <div className="flex justify-center mt-3">
                    <Pagination
                        total={pagination.totalPages}
                        page={pagination.actualPage}
                        onChange={listPosts}
                        showControls
                    />
                </div>

            </div>

            {/* Modal update blog */}
            <ModalBlog
                blog={selectedBlog}
                isOpen={isOpen}
                setIsOpen={setIsOpen}
                action={updateBlog}
            />

        </div>
    )
}