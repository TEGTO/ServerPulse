import { GetOAuthUrl } from "../..";

export interface GetOAuthUrlResponse {
    url: string;
}

export function mapGetOAuthUrlResponseToGetOAuthUrl(response: GetOAuthUrlResponse): GetOAuthUrl {
    return {
        url: response.url
    }
}