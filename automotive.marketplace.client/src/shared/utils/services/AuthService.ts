import axiosClient from "@/api/axiosClient";
import { RegisterAccountRequest } from "@/shared/types/dto/Auth/RegisterAccountRequest";

class AuthService {
  static async Register(request: RegisterAccountRequest): Promise<void> {
    const response = await axiosClient.post("/auth/register", request);
    console.log("AuthService > ", response.data);
    return response.data;
  }

  static async Refresh(): Promise<void> {
    const response = await axiosClient.post("/auth/refresh");
    console.log("AuthService > ", response);
    return response.data;
  }
}

export default AuthService;
