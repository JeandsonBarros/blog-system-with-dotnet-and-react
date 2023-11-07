import { Button, Dropdown, DropdownItem, DropdownMenu, DropdownTrigger, Input } from '@nextui-org/react';
import { CSSProperties, useContext, useEffect, useState } from 'react';
import { MdClose, MdManageAccounts, MdOutlineNewspaper, MdSearch } from 'react-icons/md';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';

import { MainContext } from '../App';
import Blog from '../shared/models/Blog';
import { axiosErrorToString } from '../shared/services/API';
import { getPublicBlog, getUserBlog } from '../shared/services/BlogService';
import HeaderStyles from '../styles/components_styles/header.module.css';

function Header() {

    const { setAlert } = useContext(MainContext)
    const params = useParams()
    const location = useLocation()
    const navigate = useNavigate()
    const [isLogged, setIsLogged] = useState<boolean>(false)
    const [blog, setBlog] = useState<Blog>()
    const [visibleSearch, setVisibleSearch] = useState<boolean>(false)
    const [search, setSearch] = useState<string>("")
    const [secondaryColor, setSecondaryColor] = useState<CSSProperties>({ color: "#026773" })

    useEffect(() => {

        if (location.pathname.slice(0, 6) == "/blog/" && params?.blogId) {
            getBlogData()
        } else {
            setBlog(undefined)
            setSecondaryColor({ color: "#026773" })
        }

        localStorage.getItem("token") ? setIsLogged(true) : setIsLogged(false)

    }, [location])

    async function getBlogData(): Promise<void> {

        try {

            const pathnameSplit = location.pathname.split("/")
            const isPreview = pathnameSplit[pathnameSplit.length - 1] == "preview" && pathnameSplit.length == 5
            const response = isPreview
                ? await getUserBlog(Number(params.blogId))
                : await getPublicBlog(Number(params.blogId))

            setBlog(response.data)
            setSecondaryColor({
                color: response.data?.titleColor ? response.data.titleColor : "#026773"
            })

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

    }

    function searchNavigation() {

        setSearch(search)

        if (blog) {
            navigate({
                pathname: `/blog/${blog.id}/${blog.title.replace(/\W/g,"-").toLowerCase()}/search`,
                search: `title=${search}`
            });
        } else {
            navigate({ pathname: "/search", search: `title=${search}` });
        }

    }

    return (
        <header className='flex flex-col shadow-lg'>

            <div
                className="flex flex-row bg-white p-4 items-center justify-between w-full"
                style={{ backgroundColor: blog?.headerColor ? blog.headerColor : "rgba(255, 255, 255, 0.39)" }}
            >

                {blog
                    ? <Link to={`/blog/${blog.id}/${blog.title.toLowerCase().replace(/\W/g,"-")}`} className="logo">
                        <h1 style={secondaryColor}>
                            {blog.title}
                        </h1>
                    </Link>
                    : <Link to="/" className="logo">
                        <h1 >Blogs</h1>
                        <MdOutlineNewspaper />
                    </Link>
                }

                <div className={`${HeaderStyles.inputSearchDesktop} w-96 mx-2`}>
                    <Input
                        value={search}
                        onValueChange={setSearch}
                        type="search"
                        placeholder="Find post"
                        endContent={
                            <button onClick={searchNavigation}>
                                <MdSearch className="text-2xl" />
                            </button>
                        }
                    />
                </div>

                <div className="flex flex-row items-center">

                    <div className={`${HeaderStyles.buttonSearchMobile} me-2`}>
                        <Button
                            style={secondaryColor}
                            variant="light"
                            title="Find post"
                            isIconOnly
                            radius="full"
                            aria-label="Options"
                            onPress={() => setVisibleSearch(!visibleSearch)}
                        >
                            {visibleSearch ? <MdClose className="text-2xl" /> : <MdSearch className="text-2xl" />}
                        </Button>
                    </div>

                    {isLogged ?
                        <Dropdown>

                            <DropdownTrigger>
                                <Button
                                    variant="light"
                                    style={secondaryColor}
                                    isIconOnly
                                    radius="full"
                                    aria-label="Options"
                                >
                                    <MdManageAccounts className="text-2xl" />
                                </Button>
                            </DropdownTrigger>

                            <DropdownMenu aria-label="Config user">

                                <DropdownItem
                                    onPress={() => navigate("/account-config")}
                                >
                                    Account config
                                </DropdownItem>

                                <DropdownItem onPress={() => navigate("/user-blogs")} >Manager blogs</DropdownItem>

                                <DropdownItem
                                    onPress={() => navigate("/users")}
                                >
                                    Users
                                </DropdownItem>

                                <DropdownItem
                                    className="text-danger"
                                    color="danger"
                                    onPress={() => {
                                        localStorage.removeItem("token")
                                        setIsLogged(false)
                                        navigate("/")
                                    }}
                                >
                                    Exit
                                </DropdownItem>

                            </DropdownMenu>

                        </Dropdown>
                        :
                        <div className="grid gap-2 grid-cols-2">

                            <Button
                                color="primary"
                                onClick={() => { navigate("/login") }}
                                variant="shadow"
                            >
                                Login
                            </Button>

                            <Button
                                color="secondary"
                                onClick={() => { navigate("/register") }}
                                variant="shadow"
                            >
                                Sign up
                            </Button>

                        </div>
                    }

                </div>

            </div>

            {visibleSearch &&
                <Input
                    className='w-full'
                    radius='none'
                    autoFocus
                    value={search}
                    onValueChange={setSearch}
                    type="search"
                    variant="faded"
                    placeholder="Find post"
                    endContent={
                        <button onClick={searchNavigation}>
                            <MdSearch className="text-2xl" />
                        </button>
                    }
                />
            }

        </header>


    );
}

export default Header;