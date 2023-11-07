import { useContext, useEffect, useState } from "react";
import User from "../../shared/models/User";
import { deleteAUser, deleteAUserProfilePhoto, findUser, getProfileImage, getUsers, patchAUser } from "../../shared/services/AuthService";
import { axiosErrorToString } from "../../shared/services/API";
import {
    Dropdown,
    DropdownTrigger,
    Button,
    DropdownMenu,
    DropdownItem,
    Progress,
    Modal,
    ModalBody,
    ModalContent,
    ModalFooter,
    ModalHeader,
    Spinner,
    Input,
    Pagination
} from "@nextui-org/react";
import { MdOutlineMoreVert, MdOutlineFileUpload, MdDelete } from "react-icons/md";
import InputPassword from "../../components/InputPassword";
import UsersStyles from "../../styles/pages_styles/auth_styles/Users.module.css";
import PersonCircle from "../../assets/img/person-circle.svg";
import UserDto from "../../shared/dtos/UserDto";
import { MainContext } from "../../App";

export default function Users() {

    const { setAlert } = useContext(MainContext)
    const [users, setUsers] = useState<User[]>()
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [pagination, setPagination] = useState<any>({ totalPages: 1, actualPage: 1 })
    const [searchUser, setSearchUser] = useState<string>()

    useEffect(() => {
        listUsers()
    }, [searchUser])

    async function listUsers(page = 1): Promise<void> {

        setIsLoad(true)

        try {
            const response = searchUser ? await findUser(searchUser, page) : await getUsers(page)
            setUsers(response.data)
            setPagination({ totalPages: response.totalPages, actualPage: response.page })
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: 'error', isVisible: true })
        }

        setIsLoad(false)

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

            <div className="flex flex-row justify-between items-center p-1">
                <h1 className="text-lg m-3">User manage</h1>
                <Input
                    className="w-40"
                    variant="underlined"
                    placeholder="Find user by email"
                    onValueChange={setSearchUser}
                />
            </div>

            <hr />

            {users && users.map(user => {
                return (
                    <UserComponent
                        key={user.id}
                        user={user}
                        users={users}
                        setUsers={setUsers}
                    />
                )
            })}

            <div className="flex justify-center mt-3">
                <Pagination
                    total={pagination.totalPages}
                    page={pagination.actualPage}
                    onChange={listUsers}
                    showControls
                />
            </div>

        </section>
    );
}

interface UserComponentPosps { user: User, users: User[], setUsers: (users: User[]) => void }
function UserComponent({ user, users, setUsers }: UserComponentPosps) {

    const { setAlert } = useContext(MainContext)
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [isImageSaved, setIsImageSaved] = useState<boolean>(false)
    const [previewImage, setPreviewImage] = useState<string>(PersonCircle)
    const [userDto, setUserDto] = useState<UserDto>({ name: user.name, email: user.email, fileProfilePicture: undefined })
    const [confirmPassword, setConfirmPassword] = useState<string>()
    const [isOpenModalData, setIsOpenModalData] = useState<boolean>(false)
    const [isOpenModalPassword, setIsOpenModalPassword] = useState<boolean>(false)
    const [isOpenModalRemove, setIsOpenModalRemove] = useState<boolean>(false)

    useEffect(() => {
      
        if (user.fileProfilePictureName) {
            setIsImageSaved(true)
            getProfileImage(user.fileProfilePictureName)
                .then(file => {
                    setPreviewImage(URL.createObjectURL(file))
                    setUserDto({ ...userDto, fileProfilePicture: file })
                })
        }
        
    }, [user.fileProfilePictureName])

    async function updateUserData(): Promise<void> {

        if (!userDto.email || !userDto.name) {
            setAlert({ text: "Don't leave fields empty.", status: "warning", isVisible: true })
            return
        }

        setIsLoad(true)

        try {

            const response = await patchAUser(user.id, { name: userDto.name, email: userDto.email, fileProfilePicture: userDto.fileProfilePicture })

            const index = users.map(userItem => userItem.id).indexOf(user.id);
            users[index] = response.data
            setUsers([...users])

            if (userDto.fileProfilePicture) setIsImageSaved(true)

            setIsOpenModalData(false)

            setAlert({ text: "Updated successfully.", status: "success", isVisible: true })

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    async function updatePassword(): Promise<void> {

        if (!confirmPassword || !userDto.password) {
            setAlert({ isVisible: true, text: "Don't leave fields empty.", status: "warning" });
            return
        }
        if (userDto.password != confirmPassword) {
            setAlert({ isVisible: true, text: "Passwords do not match.", status: "warning" });
            return
        }

        setIsLoad(true)

        try {
            await patchAUser(user.id, { password: userDto.password })
            setAlert({ text: "Password updated successfully.", status: "success", isVisible: true })
            setIsOpenModalPassword(false)
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)
    }

    async function removeAUser(): Promise<void> {

        setIsLoad(true)

        try {

            await deleteAUser(user.id)
            setAlert({ text: "User deleted.", status: "success", isVisible: true })

            const index = users.map(userItem => userItem.id).indexOf(user.id);
            users.splice(index, 1);
            setUsers([...users])

            setIsOpenModalData(false)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    async function removeProfilePhoto(): Promise<void> {

        try {

            if (isImageSaved) {
                await deleteAUserProfilePhoto(user.id)
                setAlert({ text: "Photo deleted.", status: "success", isVisible: true })

                const index = users.map(userItem => userItem.id).indexOf(user.id);
                users[index].fileProfilePictureName = undefined;
                setUsers([...users])
                setIsImageSaved(false)
            }

            setUserDto({ ...userDto, fileProfilePicture: undefined })
            setPreviewImage(PersonCircle)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

    }

    return (
        <div className="flex flex-row justify-between items-center p-2 border-b-1">

            <div className="flex flex-row items-center">

                <img className={`m-2 ${UsersStyles.userPictureSm}`} src={previewImage} />

                <div>
                    <p>{user.name}</p>
                    <small>{user.email}</small>
                </div>

            </div>

            <Dropdown>
                <DropdownTrigger>
                    <Button
                        isIconOnly
                        variant="light"
                        color="primary"
                        aria-label="Like"
                    >
                        <MdOutlineMoreVert />
                    </Button>
                </DropdownTrigger>
                <DropdownMenu aria-label="Static Actions">
                    <DropdownItem key="data" onPress={() => setIsOpenModalData(true)}>Update data</DropdownItem>
                    <DropdownItem key="password" onPress={() => setIsOpenModalPassword(true)}>Update password</DropdownItem>
                    <DropdownItem key="delete" onPress={() => setIsOpenModalRemove(true)} className="text-danger" color="danger">
                        Delete
                    </DropdownItem>
                </DropdownMenu>
            </Dropdown>

            {/* Modal to edit user */}
            <Modal
                disableAnimation
                isOpen={isOpenModalData}
                onOpenChange={() => setIsOpenModalData(!isOpenModalData)}
            >
                <ModalContent className="h-96">
                    {(onClose) => (
                        <>
                            <ModalHeader className="flex flex-col">Update user data</ModalHeader>

                            <ModalBody >

                                <div className="flex flex-col ">

                                    <div className="flex flex-row items-center mb-2">

                                        <img className={UsersStyles.userPicture} src={previewImage} />

                                        <div className="flex flex-col ms-2">

                                            <label
                                                htmlFor="profileImage"
                                                className="m-1 flex flex-row items-center justify-center bg-cyan-700 cursor-pointer rounded-lg text-white p-2"
                                            >
                                                Select an image <MdOutlineFileUpload className="ms-2" />
                                            </label>

                                            {userDto.fileProfilePicture &&
                                                <Button
                                                    className="m-1"
                                                    variant="flat"
                                                    color="danger"
                                                    onPress={removeProfilePhoto}
                                                >
                                                    Remove saved photo <MdDelete />
                                                </Button>}

                                        </div>

                                    </div>

                                    <hr />

                                    <input
                                        id="profileImage"
                                        type="file"
                                        className="hidden"
                                        onChange={event => {

                                            if (!event.target.files || event.target.files.length === 0) {
                                                setPreviewImage(PersonCircle)
                                                setUserDto({ ...userDto, fileProfilePicture: undefined })
                                                return
                                            }

                                            setPreviewImage(URL.createObjectURL(event.target.files[0]))
                                            setUserDto({ ...userDto, fileProfilePicture: event.target.files[0] })

                                            event.target.value = "";
                                        }}
                                    />

                                </div>

                                <Input
                                    value={userDto.name}
                                    placeholder="Fulano"
                                    variant="underlined"
                                    className="mb-2"
                                    onValueChange={value => setUserDto({ ...userDto, name: value })}
                                />

                                <Input
                                    type="email"
                                    value={userDto.email}
                                    placeholder="exemple@email.com"
                                    variant="underlined"
                                    className="mb-2"
                                    onValueChange={value => setUserDto({ ...userDto, email: value })}
                                />

                            </ModalBody>

                            <ModalFooter>
                                <Button color="primary" variant="light" onPress={onClose}>
                                    Cancel
                                </Button>
                                <Button color="danger" onPress={updateUserData}>
                                    {isLoad ? <Spinner color="default" size="sm" /> : <>Update</>}
                                </Button>
                            </ModalFooter>
                        </>
                    )}


                </ModalContent>
            </Modal>

            {/* Modal to edit password */}
            <Modal
                disableAnimation
                isOpen={isOpenModalPassword}
                onOpenChange={() => setIsOpenModalPassword(!isOpenModalPassword)}
            >
                <ModalContent className="h-80">
                    <ModalHeader className="flex flex-col">Update user password</ModalHeader>
                    <ModalBody >

                        <InputPassword
                            label="Password"
                            setValue={value => setUserDto({ ...userDto, password: value })}
                        />

                        <InputPassword
                            label="Confirm password"
                            setValue={value => setConfirmPassword(value)}
                        />

                    </ModalBody>
                    <ModalFooter>

                        <Button color="primary" variant="light" onPress={() => setIsOpenModalPassword(false)}>
                            Cancel
                        </Button>

                        <Button
                            color="danger"
                            onPress={updatePassword}
                        >
                            {isLoad ? <Spinner color="default" size="sm" /> : <>Update</>}
                        </Button>
                    </ModalFooter>
                </ModalContent>
            </Modal>

            {/* Modal to remove user */}
            <Modal
                disableAnimation
                isOpen={isOpenModalRemove}
                onOpenChange={() => setIsOpenModalRemove(!isOpenModalRemove)}
            >
                <ModalContent className="h-64">
                    <ModalHeader className="flex flex-col gap-1">Remove user</ModalHeader>
                    <ModalBody >

                        <p>
                            When you remove the user {user.name} from email {user.email},
                            all of their data will be permanently removed. Do you want to continue?
                        </p>

                    </ModalBody>
                    <ModalFooter>
                        <Button color="primary" variant="light" onPress={() => setIsOpenModalRemove(false)}>
                            Cancel
                        </Button>
                        <Button color="danger" onPress={removeAUser}>
                            {isLoad ? <Spinner color="default" size="sm" /> : <>Confirm</>}
                        </Button>
                    </ModalFooter>
                </ModalContent>
            </Modal>

        </div>
    )
}
